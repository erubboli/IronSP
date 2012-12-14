module IronSharePoint
  module Variation
    class << self
      attr_writer :default

      def current
        sp_context = Microsoft::SharePoint::SPContext.current
        http_context = System::Web::HttpContext.current

        variation = http_context.items["IronSP_Variation"] unless http_context.nil?

        if variation.nil?
          variation = for_web(sp_context.web) unless sp_context.nil?
          http_context.items["IronSP_Variation"] = variation unless http_context.nil?
        end

        variation || default
      end

      def for_web web
        disposables = []
        disposables << Microsoft::SharePoint::Utilities::SPMonitoredScope.new("Retrieving Variation")
        if web.is_a? String
          site = SPSite.new web
          web = site.open_web
          disposables << site << web
        end

        pweb = Microsoft::SharePoint::Publishing::PublishingWeb.GetPublishingWeb(web)
        variation = (pweb.label.try :title)

        disposables.each {|x| x.dispose}

        variation || default
      end

      def default
        @default || "en"
      end
    end
  end
end

