module IronSharePoint::Mixins
  module SPContextAccessors
    attr_writer :current_web, :current_item, :current_list, :current_site

    def current_web
      @current_web || (SPContext.current.web unless SPContext.current.nil?)
    end

    def current_item
      @current_item || (SPContext.current.item unless SPContext.current.nil?)
    end

    def current_site
      @current_site || (SPContext.current.site unless SPContext.current.nil?)
    end

    def current_list
      @current_list || (SPContext.current.list unless SPContext.current.nil?)
    end
  end
end
