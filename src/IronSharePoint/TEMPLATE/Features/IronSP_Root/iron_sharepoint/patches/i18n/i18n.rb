require 'iron_sharepoint/variation'

include IronSharePoint

module I18n
  class << self
    def translate(*args)
      options  = args.last.is_a?(Hash) ? args.pop : {}
      key      = args.shift
      backend  = config.backend
      sp_locale = IronSharePoint::Variation.current.to_sym
      locale   = options.delete(:locale) || sp_locale
      handling = options.delete(:throw) && :throw || options.delete(:raise) && :raise # TODO deprecate :raise

      raise I18n::ArgumentError if key.is_a?(String) && key.empty?

      result = catch(:exception) do
        if key.is_a?(Array)
          key.map { |k| backend.translate(locale, k, options) }
        else
          backend.translate(locale, key, options)
        end
      end
      result.is_a?(MissingTranslation) ? handle_exception(handling, result, locale, key, options) : result
    end
    alias :t :translate
  end
end

