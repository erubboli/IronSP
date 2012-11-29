require 'rubygems'
require 'test/unit'
require 'shoulda'

$LOAD_PATH.unshift(File.join(File.dirname(__FILE__), '..', 'lib'))
$LOAD_PATH.unshift(File.dirname(__FILE__))
ASSETS_DIR = File.join(File.dirname(__FILE__), "iron_templates", "assets")

require 'iron_templates'
require 'iron_templates/helper/test_view'

class Test::Unit::TestCase
end
