require 'iron_sharepoint/variation'

include IronSharePoint

module I18n
  class << self
    def translate(*args)
      options = args.pop if args.last.is_a?(Hash)
      key     = args.shift
      sp_locale = IronSharePoint::Variation.current.to_sym
      locale  = (options && options.delete(:locale)) || sp_locale
      raises  = options && options.delete(:raise)
      config.backend.translate(locale, key, options || {})
    rescue I18n::ArgumentError => exception
      raise exception if raises
      handle_exception(exception, locale, key, options)
    end
    alias :t :translate
  end
end
