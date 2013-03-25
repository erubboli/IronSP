require 'rack'

module Rack
  module Handlers
    autoload :IIS, 'iron_sharepoint/ext/rack/handlers/iron_sp'
  end
end

