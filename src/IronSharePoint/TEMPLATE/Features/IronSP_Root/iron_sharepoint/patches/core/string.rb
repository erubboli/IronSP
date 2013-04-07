class String
  def lstrip
    self.to_clr_string.trim_start.to_s
  end

  def rstrip
    self.to_clr_string.trim_end.to_s
  end

  def strip
    self.to_clr_string.trim.to_s
  end
end
