module Log4r
  class Logger
    def inspect
      self.to_s
    end

    def flush
      @outputters.each{|x| x.flush}
    end
  end
end
