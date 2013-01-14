module ActionView #:nodoc:
  class ReloadableTemplate < Template
    def mtime
      nil
    end
  end
end
