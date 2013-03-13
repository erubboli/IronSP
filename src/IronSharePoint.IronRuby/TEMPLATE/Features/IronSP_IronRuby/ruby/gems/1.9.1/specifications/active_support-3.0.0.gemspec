# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{active_support}
  s.version = "3.0.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.date = %q{2011-05-30}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.7}
  s.summary = %q{has just one dependency which points to the actual gem. this gem gets referenced by rails_config-0.1.0}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = -1

    if Gem::Version.new(Gem::VERSION) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<activesupport>, ["= 3.0.0"])
    else
      s.add_dependency(%q<activesupport>, ["= 3.0.0"])
    end
  else
    s.add_dependency(%q<activesupport>, ["= 3.0.0"])
  end
end
