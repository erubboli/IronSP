require "iron_sharepoint/view_helpers/date_helper"
require "iron_sharepoint/view_helpers/debug_helper"
require "iron_sharepoint/view_helpers/number_helper"
require "iron_sharepoint/view_helpers/raw_output_helper"
require "iron_sharepoint/view_helpers/sanitize_helper"
require "iron_sharepoint/view_helpers/tag_helper"
require "iron_sharepoint/view_helpers/text_helper"
require "iron_sharepoint/view_helpers/translation_helper"


module IronSharePoint
  module ViewHelpers
    def self.included(base)
      base.extend(ClassMethods)
    end

    module ClassMethods
      include SanitizeHelper::ClassMethods
    end

    include DateHelper
    include DebugHelper
    include NumberHelper
    include RawOutputHelper
    include SanitizeHelper
    include TagHelper
    include TextHelper
    include TranslationHelper
  end
end
