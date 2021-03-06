# monkey patch Object#inspect for nicer output
class Object
  alias_method :old_inspect, :inspect

  def inspect
    if self.respond_to? "GetDynamicMemberNames".to_clr_string
      old_inspect
    elsif self.respond_to? :GetEnumerator
      self.to_a.inspect
    else
      old_inspect
    end
  end
end

