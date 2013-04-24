module Log4r
  class Outputter
    def format(logevent)
      # Encode! the formatted string
      @formatter.format(logevent).encode!
    end
  end
end
