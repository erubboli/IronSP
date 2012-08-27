# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{partialruby}
  s.version = "0.2.0"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Dario Seminara"]
  s.date = %q{2011-04-28}
  s.email = %q{robertodarioseminara@gmail.com}
  s.extra_rdoc_files = ["README"]
  s.files = ["lib/partialruby.rb", "spec/flow_control_spec.rb", "spec/gvar_spec.rb", "spec/def_spec.rb", "spec/exception_spec.rb", "spec/call_spec.rb", "spec/const_spec.rb", "spec/ivar_spec.rb", "spec/block_spec.rb", "spec/eval_spec.rb", "spec/literal_spec.rb", "spec/class_spec.rb", "spec/operator_spec.rb", "spec/xstr_spec.rb", "LICENSE", "AUTHORS", "CHANGELOG", "README", "Rakefile", "TODO"]
  s.homepage = %q{http://github.com/tario/partialruby}
  s.rdoc_options = ["--main", "README"]
  s.require_paths = ["lib"]
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{Ruby partial interpreter written in pure-ruby}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<ruby_parser>, [">= 2.0.6"])
      s.add_runtime_dependency(%q<ruby2ruby>, [">= 1.2.5"])
    else
      s.add_dependency(%q<ruby_parser>, [">= 2.0.6"])
      s.add_dependency(%q<ruby2ruby>, [">= 1.2.5"])
    end
  else
    s.add_dependency(%q<ruby_parser>, [">= 2.0.6"])
    s.add_dependency(%q<ruby2ruby>, [">= 1.2.5"])
  end
end
