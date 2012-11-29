module IronSharePoint::Mixins
  module ParentAttributes
    def attributes
      if @attributes.nil?
        @attributes = { }
        parent.attributes.keys.each do |key|
          key_symbol = key.underscore.to_sym
          @attributes[key_symbol] = parent.attributes[key]
        end unless parent.nil?
      end

      @attributes
    end

    def has_attribute? attribute
      !attributes[attribute].nil?
    end
  end
end
