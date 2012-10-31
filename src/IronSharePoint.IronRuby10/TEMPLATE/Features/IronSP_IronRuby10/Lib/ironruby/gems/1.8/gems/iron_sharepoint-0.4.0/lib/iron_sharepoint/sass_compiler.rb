require 'sass'

module IronSharePoint
  module SassCompiler
    attr_accessor :runtime

    def runtime
      @runtime || $RUNTIME
    end

    def watch
      unless watching?
        @handler = Proc.new do |sender, args|
          if (args.event == "ItemUpdated" || args.event == "ItemAdded")
            props = args.event_properties
            extension = parse_extension props.after_url
            if ([:scss, :sass, :css].include? extension)
              compile_files
            end
          end
        end
        hive.events.add @handler
      end
    end

    def stop_watching
      if watching?
        hive.events.remove @handler
        @handler = nil
      end
    end

    def watching?
      !@handler.nil?
    end

    def compile_file url
      extension = parse_extension url
      css_url = url.gsub(/\.(sass|scss)/, ".css")
      content = hive.load_text url
      engine = Sass::Engine.new content, :syntax => extension, :load_paths => [".", "app/assets/stylesheets"]
      hive.add css_url, engine.render
    end

    def compile_files
      sass_files = hive.files.select do |x|
        x[/^app\/assets\/stylesheets\/[a-zA-Z0-0]+\.(scss|sass)/]
      end
      sass_files.each {|file| compile_file file}
    end

    def parse_extension url
      extension = url[/\.\w+/]
      unless extension.nil?
        extension = extension[1..-1].to_sym
      end
    end

    def hive
      runtime.iron_hive
    end
  end
end
