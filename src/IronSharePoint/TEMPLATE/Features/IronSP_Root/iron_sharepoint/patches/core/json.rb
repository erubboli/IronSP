load_assembly 'System.Web.Helpers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'json'

module IronSharePoint
  module JSON
    class Parser
      def initialize source, opts
        @source = source
        @symbolize_names = !!opts[:symbolize_names]
        @object_class = opts[:object_class] || Hash
        @array_class  = opts[:array_class] || Array
      end

      def parse
        convert(System::Web::Helpers::Json.decode @source)
      end

      def symbolize_names?
        @symbolize_names
      end

      private

      def convert value
        case value
        when System::Web::Helpers::DynamicJsonObject
          obj = @object_class.new
          value.get_dynamic_member_names.each do |member|
            member_value = value.send member
            member = member.to_sym if symbolize_names?
            obj[member] = convert(member_value)
          end
          obj
        when System::Web::Helpers::DynamicJsonArray
          ary = @array_class.new
          (0...value.length).each{|i| ary << convert(value[i])}
          ary
        else
          value
        end
      end
    end
  end
end

JSON.parser = IronSharePoint::JSON::Parser
