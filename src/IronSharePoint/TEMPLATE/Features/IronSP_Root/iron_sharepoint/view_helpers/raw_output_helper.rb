module IronSharePoint #:nodoc:
  module ViewHelpers #:nodoc:
    module RawOutputHelper
      def raw(stringish)
        stringish.to_s.html_safe
      end
    end
  end
end
