module Kernel
  def using obj, &blk
    result = blk.call obj if block_given?
    obj.dispose if obj.respond_to? :dispose
    result
  end
end
