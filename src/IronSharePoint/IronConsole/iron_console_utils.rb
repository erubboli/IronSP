module IronConsole
  module Utils
    def self.included base
      # monkey patch Object#inspect
      class << Object
        alias_method :old_inspect, :inspect

        def inspect
          if self.respond_to? :GetEnumerator
            self.to_a.inspect
          else
            old_inspect
          end
        end
      end
    end

    def puts obj
      (@out_buffer ||= []) << obj.to_s
      return nil
    end

    def p obj
      inspected = obj.inspect
      puts inspected
      inspected
    end

    def console_out clear=true
      ret = @out_buffer.clone
      @out_buffer = [] if clear

      return ret
    end

    def pm obj, *options
      methods = obj.methods
      methods -= Object.methods unless options.include? :more
      filter = options.select {|opt| opt.kind_of? Regexp}.first
      methods = methods.select {|name| name =~ filter} if filter

      data = methods.sort.collect do |name|
        method = obj.method(name)
        if method.arity == 0
          args = "()"
        elsif method.arity > 0
          n = method.arity
          args = "(#{(1..n).collect {|i| "arg#{i}"}.join(", ")})"
        elsif method.arity < 0
          n = -method.arity
          args = "(#{(1..n).collect {|i| "arg#{i}"}.join(", ")}, ...)"
        end
        klass = $1 if method.inspect =~ /Method: (.*?)#/
        [name, args, klass]
      end
      data.each do |item|
        puts "#{item[0]}#{item[1]} => #{item[2]}"
      end
      data.size
    end
  end
end
