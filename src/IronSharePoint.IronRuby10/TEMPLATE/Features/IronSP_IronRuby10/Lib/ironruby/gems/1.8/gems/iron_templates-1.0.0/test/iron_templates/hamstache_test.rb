require 'test_helper'

class HamstacheTest < Test::Unit::TestCase
  context "A hamstache template" do
    setup do
      @view = TestView.new [ASSETS_DIR]
    end

    context "with simple tags" do
      should "expand from_view tag" do
        result = @view.render :file => "test.hamstache"
        assert_equal "<p>View</p>", result.strip
      end

      should "expand from_context tag" do
        result = @view.render :file => "test.hamstache", :locals => {:from_context => "Context"}
        assert_equal "<p>View</p>\n<p>Context</p>", result.strip
      end
    end

    context "with a hamstache partial" do
      should "render the partial" do
        result = @view.render :file => "with_hamstache_partial.hamstache", :locals => {:from_partial => "Partial"}
        assert_equal "<p><b>Partial</b></p>", result.gsub("\n","")
      end
    end

    context "with a mustache partial" do
      should "render the partial" do
        result = @view.render :file => "with_mustache_partial.hamstache", :locals => {:from_partial => "Partial"}
        assert_equal "<p>Partial</p>", result.gsub("\n","")
      end
    end
  end
end
