# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{slim}
  s.version = "1.3.6"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Daniel Mendler", "Andrew Stone", "Fred Wu"]
  s.date = %q{2013-01-06}
  s.default_executable = %q{slimrb}
  s.description = %q{Slim is a template language whose goal is reduce the syntax to the essential parts without becoming cryptic.}
  s.email = ["mail@daniel-mendler.de", "andy@stonean.com", "ifredwu@gmail.com"]
  s.executables = ["slimrb"]
  s.files = [".gitignore", ".travis.yml", ".yardopts", "CHANGES", "Gemfile", "LICENSE", "README.md", "Rakefile", "benchmarks/context.rb", "benchmarks/profile-parser.rb", "benchmarks/profile-render.rb", "benchmarks/run-benchmarks.rb", "benchmarks/run-diffbench.rb", "benchmarks/view.erb", "benchmarks/view.haml", "benchmarks/view.slim", "bin/slimrb", "kill-travis.sh", "lib/slim.rb", "lib/slim/code_attributes.rb", "lib/slim/command.rb", "lib/slim/control_structures.rb", "lib/slim/embedded_engine.rb", "lib/slim/end_inserter.rb", "lib/slim/engine.rb", "lib/slim/filter.rb", "lib/slim/grammar.rb", "lib/slim/interpolation.rb", "lib/slim/logic_less.rb", "lib/slim/logic_less/context.rb", "lib/slim/logic_less/filter.rb", "lib/slim/parser.rb", "lib/slim/splat_attributes.rb", "lib/slim/template.rb", "lib/slim/translator.rb", "lib/slim/version.rb", "slim.gemspec", "test/core/helper.rb", "test/core/test_code_blocks.rb", "test/core/test_code_escaping.rb", "test/core/test_code_evaluation.rb", "test/core/test_code_output.rb", "test/core/test_code_structure.rb", "test/core/test_embedded_engines.rb", "test/core/test_encoding.rb", "test/core/test_html_attributes.rb", "test/core/test_html_escaping.rb", "test/core/test_html_structure.rb", "test/core/test_parser_errors.rb", "test/core/test_pretty.rb", "test/core/test_ruby_errors.rb", "test/core/test_slim_template.rb", "test/core/test_text_interpolation.rb", "test/core/test_thread_options.rb", "test/literate/TESTS.md", "test/literate/helper.rb", "test/literate/run.rb", "test/logic_less/test_logic_less.rb", "test/rails/Rakefile", "test/rails/app/controllers/application_controller.rb", "test/rails/app/controllers/parents_controller.rb", "test/rails/app/controllers/slim_controller.rb", "test/rails/app/helpers/application_helper.rb", "test/rails/app/models/child.rb", "test/rails/app/models/parent.rb", "test/rails/app/views/layouts/application.html.slim", "test/rails/app/views/parents/_form.html.slim", "test/rails/app/views/parents/edit.html.slim", "test/rails/app/views/parents/new.html.slim", "test/rails/app/views/parents/show.html.slim", "test/rails/app/views/slim/_partial.html.slim", "test/rails/app/views/slim/content_for.html.slim", "test/rails/app/views/slim/erb.html.erb", "test/rails/app/views/slim/integers.html.slim", "test/rails/app/views/slim/no_layout.html.slim", "test/rails/app/views/slim/normal.html.slim", "test/rails/app/views/slim/partial.html.slim", "test/rails/app/views/slim/thread_options.html.slim", "test/rails/app/views/slim/variables.html.slim", "test/rails/config.ru", "test/rails/config/application.rb", "test/rails/config/boot.rb", "test/rails/config/database.yml", "test/rails/config/environment.rb", "test/rails/config/environments/development.rb", "test/rails/config/environments/production.rb", "test/rails/config/environments/test.rb", "test/rails/config/initializers/backtrace_silencers.rb", "test/rails/config/initializers/inflections.rb", "test/rails/config/initializers/mime_types.rb", "test/rails/config/initializers/secret_token.rb", "test/rails/config/initializers/session_store.rb", "test/rails/config/locales/en.yml", "test/rails/config/routes.rb", "test/rails/db/migrate/20101220223037_parents_and_children.rb", "test/rails/script/rails", "test/rails/test/helper.rb", "test/rails/test/test_slim.rb", "test/translator/test_translator.rb"]
  s.homepage = %q{http://slim-lang.com/}
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{slim}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Slim is a template language.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<temple>, ["~> 0.5.5"])
      s.add_runtime_dependency(%q<tilt>, ["~> 1.3.3"])
      s.add_development_dependency(%q<rake>, [">= 0.8.7"])
      s.add_development_dependency(%q<sass>, [">= 3.1.0"])
      s.add_development_dependency(%q<minitest>, [">= 0"])
      s.add_development_dependency(%q<kramdown>, [">= 0"])
      s.add_development_dependency(%q<creole>, [">= 0"])
      s.add_development_dependency(%q<builder>, [">= 0"])
    else
      s.add_dependency(%q<temple>, ["~> 0.5.5"])
      s.add_dependency(%q<tilt>, ["~> 1.3.3"])
      s.add_dependency(%q<rake>, [">= 0.8.7"])
      s.add_dependency(%q<sass>, [">= 3.1.0"])
      s.add_dependency(%q<minitest>, [">= 0"])
      s.add_dependency(%q<kramdown>, [">= 0"])
      s.add_dependency(%q<creole>, [">= 0"])
      s.add_dependency(%q<builder>, [">= 0"])
    end
  else
    s.add_dependency(%q<temple>, ["~> 0.5.5"])
    s.add_dependency(%q<tilt>, ["~> 1.3.3"])
    s.add_dependency(%q<rake>, [">= 0.8.7"])
    s.add_dependency(%q<sass>, [">= 3.1.0"])
    s.add_dependency(%q<minitest>, [">= 0"])
    s.add_dependency(%q<kramdown>, [">= 0"])
    s.add_dependency(%q<creole>, [">= 0"])
    s.add_dependency(%q<builder>, [">= 0"])
  end
end
