require 'test_helper'

class HamlTest < Test::Unit::TestCase
  context "A haml template" do
    setup do
      @view = TestView.new [ASSETS_DIR]
    end

    context "with simple variables" do
      should "replace from_view variable" do
        result = @view.render :file => "test.haml"
        assert_equal "<p>View</p>", result.strip
      end

      should "replace from_context variable" do
        result = @view.render :file => "test.haml", :locals => {:from_context => "Context"}
        assert_equal "<p>View</p>\n<p>Context</p>", result.strip
      end
    end

    context "with a partial" do
      should "render the partial" do
        result = @view.render :file => "with_partial.haml"
        assert_equal "<p>Partial</p>", result.strip
      end
    end

    context "that is a partial" do
      should "render the partial" do
        result = @view.render 'partial', :from_locals => "Partial"
        assert_equal "<p>Partial</p>", result.strip
      end
    end
  end
end
