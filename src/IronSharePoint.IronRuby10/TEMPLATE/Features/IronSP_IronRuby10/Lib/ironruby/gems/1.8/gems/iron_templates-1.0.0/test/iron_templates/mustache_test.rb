require 'test_helper'

class MustacheTest < Test::Unit::TestCase
  context "A mustache template" do
    setup do
      @view = TestView.new [ASSETS_DIR]
    end

    context "with simple tags" do
      should "expand from_view tag" do
        result = @view.render :file => "test.mustache"
        assert_equal "View", result.strip
      end

      should "expand from_context tag" do
        result = @view.render :file => "test.mustache", :locals => {:from_context => "Context"}
        assert_equal "View\nContext", result.strip
      end
    end

    context "with a partial" do
      should "render the partial" do
        result = @view.render :file => "with_partial.mustache", :locals => {:from_partial => "Partial"}
        assert_equal "Partial", result.strip
      end
    end
  end
end
