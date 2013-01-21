# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{temple}
  s.version = "0.5.5"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Magnus Holm", "Daniel Mendler"]
  s.date = %q{2012-10-16}
  s.email = ["judofyr@gmail.com", "mail@daniel-mendler.de"]
  s.files = [".gitignore", ".travis.yml", ".yardopts", "CHANGES", "EXPRESSIONS.md", "Gemfile", "LICENSE", "README.md", "Rakefile", "lib/temple.rb", "lib/temple/engine.rb", "lib/temple/erb/engine.rb", "lib/temple/erb/parser.rb", "lib/temple/erb/template.rb", "lib/temple/erb/trimming.rb", "lib/temple/exceptions.rb", "lib/temple/filter.rb", "lib/temple/filters/control_flow.rb", "lib/temple/filters/dynamic_inliner.rb", "lib/temple/filters/eraser.rb", "lib/temple/filters/escapable.rb", "lib/temple/filters/multi_flattener.rb", "lib/temple/filters/static_merger.rb", "lib/temple/filters/validator.rb", "lib/temple/generators.rb", "lib/temple/grammar.rb", "lib/temple/hash.rb", "lib/temple/html/attribute_merger.rb", "lib/temple/html/attribute_remover.rb", "lib/temple/html/attribute_sorter.rb", "lib/temple/html/dispatcher.rb", "lib/temple/html/fast.rb", "lib/temple/html/filter.rb", "lib/temple/html/pretty.rb", "lib/temple/mixins/dispatcher.rb", "lib/temple/mixins/engine_dsl.rb", "lib/temple/mixins/grammar_dsl.rb", "lib/temple/mixins/options.rb", "lib/temple/mixins/template.rb", "lib/temple/parser.rb", "lib/temple/templates.rb", "lib/temple/templates/rails.rb", "lib/temple/templates/tilt.rb", "lib/temple/utils.rb", "lib/temple/version.rb", "temple.gemspec", "test/filters/test_control_flow.rb", "test/filters/test_dynamic_inliner.rb", "test/filters/test_eraser.rb", "test/filters/test_escapable.rb", "test/filters/test_multi_flattener.rb", "test/filters/test_static_merger.rb", "test/helper.rb", "test/html/test_attribute_merger.rb", "test/html/test_attribute_remover.rb", "test/html/test_attribute_sorter.rb", "test/html/test_fast.rb", "test/html/test_pretty.rb", "test/mixins/test_dispatcher.rb", "test/mixins/test_grammar_dsl.rb", "test/test_engine.rb", "test/test_erb.rb", "test/test_filter.rb", "test/test_generator.rb", "test/test_grammar.rb", "test/test_hash.rb", "test/test_utils.rb"]
  s.homepage = %q{https://github.com/judofyr/temple}
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Template compilation framework in Ruby}
  s.test_files = ["test/filters/test_control_flow.rb", "test/filters/test_dynamic_inliner.rb", "test/filters/test_eraser.rb", "test/filters/test_escapable.rb", "test/filters/test_multi_flattener.rb", "test/filters/test_static_merger.rb", "test/helper.rb", "test/html/test_attribute_merger.rb", "test/html/test_attribute_remover.rb", "test/html/test_attribute_sorter.rb", "test/html/test_fast.rb", "test/html/test_pretty.rb", "test/mixins/test_dispatcher.rb", "test/mixins/test_grammar_dsl.rb", "test/test_engine.rb", "test/test_erb.rb", "test/test_filter.rb", "test/test_generator.rb", "test/test_grammar.rb", "test/test_hash.rb", "test/test_utils.rb"]

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_development_dependency(%q<tilt>, [">= 0"])
      s.add_development_dependency(%q<bacon>, [">= 0"])
      s.add_development_dependency(%q<rake>, [">= 0"])
    else
      s.add_dependency(%q<tilt>, [">= 0"])
      s.add_dependency(%q<bacon>, [">= 0"])
      s.add_dependency(%q<rake>, [">= 0"])
    end
  else
    s.add_dependency(%q<tilt>, [">= 0"])
    s.add_dependency(%q<bacon>, [">= 0"])
    s.add_dependency(%q<rake>, [">= 0"])
  end
end
