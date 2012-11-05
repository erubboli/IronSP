module IronSharePoint::Mixins
  module TypeRegistration
    def self.included klass
      dot_net_type = klass.name.gsub("::",".")
      $RUNTIME.register_dynamic_type(dot_net_type, klass)
      IRON_DEFAULT_LOGGER.debug "Registered #{klass} as #{dot_net_type}"
    end
  end
end
