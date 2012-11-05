module IronSharePoint
  class IronView < ActionView::Base
    def context= context
      meta = class<<self;self;end
      context.each do |k,v|
        meta.send :define_method, k.to_sym do
          v
        end
      end
    end
  end
end
