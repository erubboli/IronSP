# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{iron_templates}
  s.version = "1.0.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Kevin Mees"]
  s.date = %q{2012-11-02}
  s.description = %q{Adds support for Haml, Mustache and Hamstache Templates in ActionViews for IronSharePoint}
  s.email = %q{kev.mees@gmail.com}
  s.files = ["README.md", "Rakefile", "lib/iron_templates/haml.rb", "lib/iron_templates/hamstache.rb", "lib/iron_templates/mustache.rb", "lib/iron_templates/version.rb", "lib/iron_templates.rb", "test/iron_templates/assets/_partial.haml", "test/iron_templates/assets/partial.hamstache", "test/iron_templates/assets/partial.mustache", "test/iron_templates/assets/test.haml", "test/iron_templates/assets/test.hamstache", "test/iron_templates/assets/test.mustache", "test/iron_templates/assets/with_hamstache_partial.hamstache", "test/iron_templates/assets/with_mustache_partial.hamstache", "test/iron_templates/assets/with_partial.haml", "test/iron_templates/assets/with_partial.mustache", "test/iron_templates/haml_test.rb", "test/iron_templates/hamstache_test.rb", "test/iron_templates/helper/test_view.rb", "test/iron_templates/mustache_test.rb", "test/test_helper.rb"]
  s.homepage = %q{http://github.com/expertsinside/iron_templates}
  s.require_paths = [".", "lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Iron Templates: More Templates in IronSharePoint}
  s.test_files = ["test/iron_templates/assets/_partial.haml", "test/iron_templates/assets/partial.hamstache", "test/iron_templates/assets/partial.mustache", "test/iron_templates/assets/test.haml", "test/iron_templates/assets/test.hamstache", "test/iron_templates/assets/test.mustache", "test/iron_templates/assets/with_hamstache_partial.hamstache", "test/iron_templates/assets/with_mustache_partial.hamstache", "test/iron_templates/assets/with_partial.haml", "test/iron_templates/assets/with_partial.mustache", "test/iron_templates/haml_test.rb", "test/iron_templates/hamstache_test.rb", "test/iron_templates/helper/test_view.rb", "test/iron_templates/mustache_test.rb", "test/test_helper.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
