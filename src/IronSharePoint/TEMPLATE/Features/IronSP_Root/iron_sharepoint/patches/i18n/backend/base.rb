require 'iron_sharepoint/patches/core/string'

module I18n
  module Backend
    module Base

      protected

      def load_yml(filename)
        yaml = YAML::load(File.read filename)
        convert_to_utf8 yaml
      end

      def convert_to_utf8 translation
        case translation
        when String, System::String
          bytes = System::Text::Encoding.Default.get_bytes translation
          bytes = System::Text::Encoding.convert(System::Text::Encoding.Default, System::Text::Encoding.UTF8, bytes)
          System::Text::Encoding.UTF8.get_string(bytes)
        when Array
          translation.map do |x|
            convert_to_utf8 x
          end
        when Hash
          converted = {}
          translation.each do |k,v|
            converted[k] = convert_to_utf8 v
          end
          converted
        end
      end
    end
  end
end
