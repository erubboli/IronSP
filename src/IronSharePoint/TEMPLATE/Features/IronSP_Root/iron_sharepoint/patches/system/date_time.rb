module System
  class DateTime
    def httpdate
      self.to_universal_time.to_string("r")
    end
  end
end