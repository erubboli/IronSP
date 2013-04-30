require 'iron_sharepoint/variation'
require 'i18n/config'

module I18n
  class Config
    # The only configuration value that is not global and scoped to thread is :locale.
    # It defaults to the default_locale.
    def locale
      @locale || default_locale
    end

    # Sets the current locale pseudo-globally, i.e. in the Thread.current hash.
    def locale=(locale)
      @locale = locale.to_sym rescue nil
    end

    # Returns the current default locale. Defaults to IronSP::Variation.current
    def default_locale
      @@default_locale || IronSP::Variation.current
    end

    # Sets the current default locale. Used to set a custom default locale.
    def default_locale=(locale)
      @@default_locale = locale.to_sym rescue nil
    end
  end
end

