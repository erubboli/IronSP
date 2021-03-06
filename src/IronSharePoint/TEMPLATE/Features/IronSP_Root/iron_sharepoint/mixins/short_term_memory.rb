module IronSharePoint::Mixins
  module ShortTermMemory
    include IronSharePoint::Mixins::Logging

    DEFAULT_CACHE_SETTINGS = {:expiration_time => 900, :anonymous_only => true}

    def cache_settings settings = nil
      if settings.nil?
        @cache_settings || DEFAULT_CACHE_SETTINGS
      else
        @cache_settings = cache_settings.merge settings
      end
    end

    # def remember_all
    #   prime_cache ".*"
    # end

    def forget_all
      flush_cache ".*"
    end

    # def prime_cache(*syms)
    #   syms.each do |sym|
    #     methods.each do |m|
    #       if m.to_s =~ /^_unmemoized_(#{sym})/
    #         if method(m).arity == 0
    #           __send__($1)
    #         else
    #           ivar = IronSharePoint::Mixins::ShortTermMemory.memoized_ivar_for($1)
    #           instance_variable_set(ivar, {})
    #         end
    #       end
    #     end
    #   end
    # end

    def flush_cache(*syms)
      syms.each do |sym|
        (methods + private_methods + protected_methods).each do |m|
          if m.to_s =~ /^_unremembered_(#{sym.to_s.gsub(/\?\Z/, '\?')})/
            [true, false].each do |b|
              cache_key = cache_key_for $1, b
              invalidate cache_key
            end
          end
        end
      end
    end

    def cremember(*symbols)
      symbols.each do |symbol|
        original_method = :"_unremembered_#{symbol}"

        instance_eval <<-EOS, __FILE__, __LINE__ + 1
          if !methods.include?('#{original_method}')
            alias #{original_method} #{symbol}

            if method(:#{symbol}).arity == 0
              def #{symbol}(reload = false)
                cache_key = self.cache_key_for :#{symbol}
                result = self.fetch cache_key
                if reload || result.nil?
                  result = #{original_method}
                  self.store cache_key, result
                end
                return result
              end
            else
              def #{symbol}(*args)
                cache_key = self.cache_key_for :#{symbol}
                args_length = method(:#{original_method}).arity
                if args.length == args_length + 1 &&
                  (args.last == true || args.last == :reload)
                  reload = args.pop
                end

                result = self.fetch cache_key, args
                if reload || result.nil?
                  result = #{original_method}(*args)
                  self.store cache_key, result, args
                end
                return result
              end
            end
          end
        EOS
      end
    end

    def remember(*symbols)
      symbols.each do |symbol|
        original_method = :"_unremembered_#{symbol}"

        class_eval <<-EOS, __FILE__, __LINE__ + 1
          if !method_defined?(:#{original_method})
            alias #{original_method} #{symbol}

            if instance_method(:#{symbol}).arity == 0
              def #{symbol}(reload = false)
                cache_key = self.class.cache_key_for :#{symbol}
                result = self.class.fetch cache_key
                if reload || result.nil?
                  result = #{original_method}
                  self.class.store cache_key, result
                end
                return result
              end
            else
              def #{symbol}(*args)
                cache_key = self.class.cache_key_for :#{symbol}
                args_length = method(:#{original_method}).arity
                if args.length == args_length + 1 &&
                  (args.last == true || args.last == :reload)
                  reload = args.pop
                end

                result = self.class.fetch cache_key, args
                if reload || result.nil?
                  result = #{original_method}(*args)
                  self.class.store cache_key, result, args
                end
                return result
              end
            end
          end
        EOS
      end
    end

    def cache_key_for symbol
      if symbol.is_a? Symbol
        variation = IronSharePoint::Variation.current
        if self.is_a? Module
          class_name = self.name
          seperator = (self.method_defined? symbol) ? "." : "#"
        else
          class_name = self.class.name
          seperator = (self.respond_to? symbol) ? "." : "#"
        end

        "#{variation}_#{class_name}#{seperator}#{symbol}"
      else
        symbol
      end
    end

    def store key, data, args = []
      return if data.nil?

      key = cache_key_for key
      if http_cache
        hash = http_cache[key] || {}
        hash[args] = data
        http_cache.insert key, hash, nil,
          System::DateTime.utc_now.add_seconds(cache_settings[:expiration_time]),
          System::Web::Caching::Cache.no_sliding_expiration
        logger.debug "Remembered #{key} in HttpCache"
      elsif http_context
        hash = http_context.items[key] || {}
        hash[args] = data
        http_context.items[key] = hash
        logger.debug "Remembered #{key} in HttpContext"
      end
      key
    end

    def fetch key, args = []
      key = cache_key_for key
      if http_cache
        if is_invalidated? key
          http_cache.remove key
          invalidated_keys.delete key
          logger.debug "Cleared cache for #{key}"
          nil
        else
          (http_cache[key] || {})[args]
        end
      elsif http_context
        (http_context.items[key] || {})[args]
      end
    end

    def invalidate key
      key = cache_key_for key
      invalidated_keys.add key
      logger.debug "Invalidated #{key} in HttpCache"
      key
    end

    def invalidated_keys
      @invalidated_keys ||= Set.new
    end

    def is_invalidated? key
      key = cache_key_for key
      invalidated_keys.include? key
    end

    protected

    def must_cache?
      !cache_settings.fetch(:anonymous_only, true) || SPContext.current.web.current_user.nil?
    end

    def http_cache
      if http_context && must_cache?
        http_context.cache
      end
    end

    def http_context
      System::Web::HttpContext.current
    end
  end
end
