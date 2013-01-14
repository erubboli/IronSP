class System::String
  def to_json(options = nil) #:nodoc:
    ActiveSupport::JSON::Encoding.escape(self.to_s)
  end

  def as_json(options = nil) #:nodoc:
    self.to_s
  end
end
