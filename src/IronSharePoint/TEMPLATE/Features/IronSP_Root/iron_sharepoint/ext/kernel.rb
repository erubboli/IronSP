module Kernel
  def using obj, &blk
    begin
      result = blk.call obj if block_given?
    ensure
      obj.dispose if obj.respond_to? :dispose
    end
    result
  end

  def monitor name = "Unnamed", &blk
    using Microsoft::SharePoint::Utilities::SPMonitoredScope.new(name), &blk
  end
end
