module IronSharePoint
  module Console
    module Utils
      def puts obj
        out_buffer << obj.to_s
        return nil
      end

      def p obj
        inspected = obj.inspect
        puts inspected
        inspected
      end

      def out seperator = '\n', clear = true
        response = out_buffer.join seperator
        out_buffer.clear
        response
      end

      def pm obj, *options
        methods = obj.methods
        methods -= Object.methods unless options.include? :more
        filter = options.select {|opt| opt.kind_of? Regexp}.first
        methods = methods.select {|name| name =~ filter} if filter

        data = methods.sort.collect do |name|
          begin
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
          rescue Exception => ex
            nil
          end
        end

        data.compact.each do |item|
          puts "#{item[0]}#{item[1]} => #{item[2]}"
        end

        data.size
      end

      private

      def out_buffer
        $out_buffer ||= []
      end
    end
  end
end
