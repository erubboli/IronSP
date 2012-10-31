# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{iron_templates}
  s.version = "0.9.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Kevin Mees"]
  s.date = %q{2012-10-26}
  s.description = %q{Adds support for Haml, Mustache and Hamstache Templates in ActionViews for IronSharePoint}
  s.email = %q{kev.mees@gmail.com}
  s.files = ["README.md", "Rakefile", "lib/iron_templates/haml.rb", "lib/iron_templates/hamstache.rb", "lib/iron_templates/mustache.rb", "lib/iron_templates.rb"]
  s.homepage = %q{http://github.com/expertsinside/iron_templates}
  s.require_paths = [".", "lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Iron Templates: More Templates in IronSharePoint}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
