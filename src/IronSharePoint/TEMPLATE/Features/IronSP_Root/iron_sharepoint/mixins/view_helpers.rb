require "iron_sharepoint/view_helpers/tag_helper"
require "iron_sharepoint/view_helpers/debug_helper"
require "iron_sharepoint/view_helpers/translation_helper"

module IronSharePoint
  module Mixins
    module ViewHelpers
      include ActionView::Helpers::TagHelper
      include ActionView::Helpers::TranslationHelper
      include ActionView::Helpers::DebugHelper
    end
  end
end
