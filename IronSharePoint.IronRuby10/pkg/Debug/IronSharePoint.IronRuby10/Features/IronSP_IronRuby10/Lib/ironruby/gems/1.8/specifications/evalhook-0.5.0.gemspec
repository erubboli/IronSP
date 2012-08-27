# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{evalhook}
  s.version = "0.5.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Dario Seminara"]
  s.date = %q{2011-06-20}
  s.email = %q{robertodarioseminara@gmail.com}
  s.extra_rdoc_files = ["README"]
  s.files = ["examples/example1.rb", "examples/example3.rb", "examples/example5.rb", "examples/example4.rb", "examples/example2.rb", "lib/evalhook/multi_hook_handler.rb", "lib/evalhook/tree_processor.rb", "lib/evalhook/redirect_helper.rb", "lib/evalhook.rb", "spec/hook_handler/hook_handler_arguments_spec.rb", "spec/hook_handler/hook_handler_hook_spec.rb", "spec/hook_handler/hook_handler_multiple_redirect_spec.rb", "spec/hook_handler/hook_handler_multiple_spec.rb", "spec/hook_handler/hook_handler_visitor_spec.rb", "spec/hook_handler/hook_handler_ruby_spec.rb", "spec/hook_handler/hook_handler_package_spec.rb", "spec/hook_handler/hook_handler_defaults_spec.rb", "spec/validation/hook_handler_spec.rb", "LICENSE", "AUTHORS", "CHANGELOG", "README", "Rakefile", "TODO"]
  s.homepage = %q{http://github.com/tario/evalhook}
  s.rdoc_options = ["--main", "README"]
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Alternate eval which hook all methods executed in the evaluated code}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<partialruby>, [">= 0.2.0"])
      s.add_runtime_dependency(%q<ruby_parser>, [">= 2.0.6"])
    else
      s.add_dependency(%q<partialruby>, [">= 0.2.0"])
      s.add_dependency(%q<ruby_parser>, [">= 2.0.6"])
    end
  else
    s.add_dependency(%q<partialruby>, [">= 0.2.0"])
    s.add_dependency(%q<ruby_parser>, [">= 2.0.6"])
  end
end
