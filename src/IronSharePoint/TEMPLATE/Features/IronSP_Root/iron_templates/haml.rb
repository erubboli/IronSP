require 'haml'

# RAILS_ROOT must be set for Haml.init_rails to work
RAILS_ROOT = Dir.pwd unless defined?(RAILS_ROOT)

Haml.init_rails(binding) if defined?(Haml)
