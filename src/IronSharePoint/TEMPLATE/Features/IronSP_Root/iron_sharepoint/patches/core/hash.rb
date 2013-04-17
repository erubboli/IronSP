class Hash
  # Sane hash keys
  def hash
    inject(0) {|hash,pair| hash ^ pair.hash}
  end

  def eql?(other)
    self == other
  end
end
