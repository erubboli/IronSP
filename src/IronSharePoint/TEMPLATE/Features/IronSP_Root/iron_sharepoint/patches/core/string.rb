class System::String
  def to_json(options = nil) #:nodoc:
    ActiveSupport::JSON::Encoding.escape(self.to_s)
  end

  def as_json(options = nil) #:nodoc:
    self.to_s
  end

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
