# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{iron_sharepoint}
  s.version = "0.3.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Kevin Mees"]
  s.date = %q{2012-10-31}
  s.description = %q{Adds dynamic iron languages to SharePoint}
  s.email = %q{kev.mees@gmail.com}
  s.files = ["README.md", "Rakefile", "lib/iron_sharepoint/coffee_compiler.rb", "lib/iron_sharepoint/iron_composite_control.rb", "lib/iron_sharepoint/iron_control.rb", "lib/iron_sharepoint/iron_view.rb", "lib/iron_sharepoint/main_http_handler.rb", "lib/iron_sharepoint/mixins/control_view.rb", "lib/iron_sharepoint/mixins/logging.rb", "lib/iron_sharepoint/mixins/parent_attributes.rb", "lib/iron_sharepoint/mixins/type_registration.rb", "lib/iron_sharepoint/sass_compiler.rb", "lib/iron_sharepoint.rb", "lib/log4r/outputter/iron_logs_outputter.rb", "lib/log4r/outputter/iron_runtime_outputter.rb"]
  s.homepage = %q{http://github.com/expertsinside/iron_sharepoint}
  s.require_paths = [".", "lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{IronSharePoint}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
