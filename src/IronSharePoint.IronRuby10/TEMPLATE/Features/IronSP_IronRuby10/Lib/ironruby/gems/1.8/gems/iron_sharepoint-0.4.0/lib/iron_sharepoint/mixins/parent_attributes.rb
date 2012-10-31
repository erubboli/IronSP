module IronSharePoint::Mixins
  module ParentAttributes
    def attributes
      return { } if parent.nil?

      parent.attributes.keys.each do |key|
        key_symbol = key.to_snake_case.to_sym
        (@attributes ||= {})[key_symbol] = parent.attributes[key]
      end if @attributes.nil?

      @attributes
    end

    def has_attribute? attribute
      !attributes[attribute].nil?
    end
  end
end
