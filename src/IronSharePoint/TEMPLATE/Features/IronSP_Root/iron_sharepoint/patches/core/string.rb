class String

  # Use .NET pendant for the strip methods to avoid ArgumentOutOfRangeExceptions when stripping strings with special characters
  def lstrip
    self.to_clr_string.trim_start.to_s
  end

  def rstrip
    self.to_clr_string.trim_end.to_s
  end

  def strip
    self.to_clr_string.trim.to_s
  end

  # Mostly taken from https://github.com/rubinius/rubinius/blob/master/kernel/common/string.rb
  def partition(pattern=nil)
    return super() if pattern == nil && block_given?

    if pattern.kind_of? Regexp
      if m = pattern.match(self)
        return [m.pre_match, m.to_s, m.post_match]
      end
    else
      pattern = pattern.to_s
      if i = index(pattern)
        post_start = i + pattern.length
        post_len = size - post_start

        return [substring(0, i),
                pattern.dup,
                substring(post_start, post_len)]
      end
    end

    # Nothing worked out, this is the default.
    return [self, "", ""]
  end

  def rpartition(pattern)
    if pattern.kind_of? Regexp
      if i = rindex(pattern)
        m = pattern.match self[i..-1]
        [self[0...i], m[0], m.post_match]
      end
    else
      pattern = pattern.to_s
      if i = rindex(pattern)
        post_start = i + pattern.length
        post_len = size - post_start

        return [substring(0, i),
                pattern.dup,
                substring(post_start, post_len)]
      end

      # Nothing worked out, this is the default.
      return ["", "", self]
    end
  end
end
