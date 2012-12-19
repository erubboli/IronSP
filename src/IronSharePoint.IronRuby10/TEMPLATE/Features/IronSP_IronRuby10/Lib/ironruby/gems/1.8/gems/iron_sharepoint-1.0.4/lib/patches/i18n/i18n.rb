require 'lib/iron_sharepoint/variation'

include IronSharePoint

module I18n
  class << self
    def translate(*args)
      options = args.pop if args.last.is_a?(Hash)
      key     = args.shift
      sp_locale = IronSharePoint::Variation.current.to_sym
      locale  = (options && options.delete(:locale)) || sp_locale
      raises  = options && options.delete(:raise)
      translation = config.backend.translate(locale, key, options || {})
      convert_to_utf8 translation
    rescue I18n::ArgumentError => exception
      raise exception if raises
      handle_exception(exception, locale, key, options)
    end
    alias :t :translate

    def convert_to_utf8 translation
      case translation
      when String
        bytes = System::Text::Encoding.Default.get_bytes translation
        bytes = System::Text::Encoding.convert(System::Text::Encoding.Default, System::Text::Encoding.UTF8, bytes)
        System::Text::Encoding.UTF8.get_string bytes
      when Array
        translation.dup.map do |x|
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
