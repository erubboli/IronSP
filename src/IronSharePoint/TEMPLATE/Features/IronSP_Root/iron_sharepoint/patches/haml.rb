require 'haml'

module Haml
  module Filters
    # Does not parse the filtered text.
    # This is useful for large blocks of text without HTML tags,
    # when you don't want lines starting with `.` or `-`
    # to be parsed.
    module Plain
      include Base

      # @see Base#render
      def render(text); text; end
    end

    # Surrounds the filtered text with `<script>` and CDATA tags.
    # Useful for including inline Javascript.
    module Javascript
      include Base

      # @see Base#render_with_options
      def render_with_options(text, options)
        <<END
<script type=#{options[:attr_wrapper]}text/javascript#{options[:attr_wrapper]}>
  //<![CDATA[
    #{text.to_clr_string.TrimEnd().Replace("\n", "\n    ")}
  //]]>
</script>
END
      end
    end

    # Surrounds the filtered text with `<style>` and CDATA tags.
    # Useful for including inline CSS.
    module Css
      include Base

      # @see Base#render_with_options
      def render_with_options(text, options)
        <<END
<style type=#{options[:attr_wrapper]}text/css#{options[:attr_wrapper]}>
  /*<![CDATA[*/
    #{text.to_clr_string.TrimEnd().Replace("\n", "\n    ")}
  /*]]>*/
</style>
END
      end
    end

    # Surrounds the filtered text with CDATA tags.
    module Cdata
      include Base

      # @see Base#render
      def render(text)
        "<![CDATA[#{("\n" + text).to_clr_string.TrimEnd().Replace("\n", "\n    ")}\n]]>"
      end
    end
  end

  module Precompiler
    alias_method :process_line_without_mustache, :process_line

    def process_line(text, index)
      if text =~ /^\{\{.*\}\}$/
        @index = index + 1
        push_merged_text(text, 1, true)
        concat_merged_text("\n")
        return
      else
        process_line_without_mustache(text, index)
      end
    end
  end
end

