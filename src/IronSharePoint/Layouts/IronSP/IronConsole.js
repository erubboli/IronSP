(function() {
  var IronConsole, IronConsoleView,
    _this = this,
    __slice = [].slice;

  IronConsole = (function() {
    var mod;

    function IronConsole(serviceUrl, options) {
      var _this = this;
      this.serviceUrl = serviceUrl;
      if (options == null) {
        options = {};
      }
      this.wrapExpression = function(expression) {
        return IronConsole.prototype.wrapExpression.apply(_this, arguments);
      };
      this.addToHistory = function(expression) {
        return IronConsole.prototype.addToHistory.apply(_this, arguments);
      };
      this.lastResultVariable = options['lastResultVariable'] || '_';
      this.history = [];
      this.successCallbacks = [];
      this.errorCallbacks = [];
    }

    IronConsole.prototype.execute = function(expression) {
      var url_params, wrapped,
        _this = this;
      this.addToHistory(expression);
      wrapped = this.wrapExpression(expression);
      url_params = location.search.substring(1);
      if (url_params) {
        url_params = "&" + url_params;
      }
      return $.ajax({
        type: 'POST',
        dataType: 'text',
        data: {
          script: wrapped
        },
        url: this.serviceUrl + url_params,
        success: function(json) {
          var cb, result, _i, _j, _len, _len1, _ref, _ref1, _results, _results1;
          result = $.parseJSON(json);
          if (!result["HasError"]) {
            _ref = _this.successCallbacks;
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              cb = _ref[_i];
              _results.push(cb(result));
            }
            return _results;
          } else {
            _ref1 = _this.errorCallbacks;
            _results1 = [];
            for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
              cb = _ref1[_j];
              _results1.push(cb(result["Error"]));
            }
            return _results1;
          }
        },
        error: function() {
          var args, cb, _i, _len, _ref, _results;
          args = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
          _ref = _this.errorCallbacks;
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            cb = _ref[_i];
            _results.push(cb.apply(null, args));
          }
          return _results;
        }
      });
    };

    IronConsole.prototype.getExpressionFromHistory = function(index) {
      if (this.history.length === 0) {
        return '';
      }
      return this.history[this.history.length - 1 - mod(index, this.history.length)];
    };

    IronConsole.prototype.onExecuteSuccess = function(callback) {
      return this.successCallbacks.push(callback);
    };

    IronConsole.prototype.onExecuteError = function(callback) {
      return this.errorCallbacks.push(callback);
    };

    IronConsole.prototype.addToHistory = function(expression) {
      var escaped;
      escaped = expression.replace(/\\n$/, '');
      return this.history.push(escaped);
    };

    IronConsole.prototype.wrapExpression = function(expression) {
      if (this.lastResultVariable != null) {
        return expression;
      } else {
        return expression;
      }
    };

    mod = function(n, base) {
      return ((n % base) + base) % base;
    };

    return IronConsole;

  })();

  IronConsoleView = (function() {

    function IronConsoleView(console, options) {
      var _this = this;
      this.console = console != null ? console : new IronConsole;
      if (options == null) {
        options = {};
      }
      this.showExecuting = function(b) {
        if (b == null) {
          b = true;
        }
        return IronConsoleView.prototype.showExecuting.apply(_this, arguments);
      };
      this.toggleEditMode = function() {
        return IronConsoleView.prototype.toggleEditMode.apply(_this, arguments);
      };
      this.scrollToPrompt = function() {
        return IronConsoleView.prototype.scrollToPrompt.apply(_this, arguments);
      };
      this.append = function(type, prefix, text) {
        return IronConsoleView.prototype.append.apply(_this, arguments);
      };
      this.insertTab = function() {
        return IronConsoleView.prototype.insertTab.apply(_this, arguments);
      };
      this.registerEventHandlers = function() {
        return IronConsoleView.prototype.registerEventHandlers.apply(_this, arguments);
      };
      this.render = function() {
        return IronConsoleView.prototype.render.apply(_this, arguments);
      };
      this.resultPrefix = options["resultPrefix"] || '=>';
      this.inputPrefix = options["inputPrefix"] || '>>';
      this.outputPrefix = options["outputPrefix"] || '';
      this.$container = $(options["containerSelector"] || '#ironSP-console-container');
      this.consoleTemplate = $(options["consoleTemplateSelector"] || '#ironSP-console-template').html();
      this.consoleLineTemplate = $(options["consoleLineTemplateSelector"] || '#ironSP-console-line-template').html();
      this.historyIndex = 0;
      this.editMode = false;
      this.render();
      this.registerEventHandlers();
    }

    IronConsoleView.prototype.render = function() {
      this.$console = $(Mustache.render(this.consoleTemplate, {
        prompt: this.inputPrefix
      }));
      return this.$container.empty().append(this.$console);
    };

    IronConsoleView.prototype.registerEventHandlers = function() {
      var _this = this;
      this.console.onExecuteSuccess(function(response) {
        if (response["Output"]) {
          _this.append("output", _this.outputPrefix, response["Output"]);
        }
        _this.append("result", _this.resultPrefix, "[" + response["ExecutionTime"] + "ms] " + response["ReturnValue"]);
        return _this.showExecuting(false);
      });
      this.console.onExecuteError(function(error, stackTrace) {
        _this.append("error", '', error);
        return _this.showExecuting(false);
      });
      this.$input || (this.$input = $("#ironSP-console-input"));
      return this.$input.keydown(function(e) {
        var expression, handled, _ref;
        expression = (_ref = _this.$input.val()) != null ? _ref.trim() : void 0;
        handled = true;
        if (!_this.editMode) {
          switch (e.keyCode) {
            case 13:
              if (expression === 'clear') {
                _this.clearConsole();
              } else if (expression !== '') {
                _this.append("input", _this.inputPrefix, expression);
                _this.historyIndex = 0;
                _this.clearInput();
                _this.showExecuting();
                _this.console.execute(expression);
              }
              break;
            case 9:
              _this.insertTab();
              break;
            case 38:
              _this.$input.val(_this.console.getExpressionFromHistory(_this.historyIndex));
              _this.historyIndex += 1;
              break;
            case 40:
              _this.historyIndex -= 1;
              _this.$input.val(_this.console.getExpressionFromHistory(_this.historyIndex));
              break;
            case 45:
            case 126:
              _this.toggleEditMode();
              break;
            default:
              handled = false;
          }
        } else {
          switch (e.keyCode) {
            case 9:
              _this.insertTab();
              break;
            case 45:
            case 126:
              _this.toggleEditMode();
              break;
            default:
              handled = false;
          }
        }
        return !handled;
      });
    };

    IronConsoleView.prototype.insertTab = function() {
      return this.$input.replaceSelection("    ");
    };

    IronConsoleView.prototype.clearInput = function() {
      return this.$input.val('');
    };

    IronConsoleView.prototype.clearConsole = function() {
      this.clearInput();
      return this.$console.find(".ironSP-console-line").remove();
    };

    IronConsoleView.prototype.append = function(type, prefix, text) {
      var $line, i, line, linePrefix, _i, _len, _ref;
      text || (text = '');
      _ref = text.replace(/\r/gm, '\n').split('\n');
      for (i = _i = 0, _len = _ref.length; _i < _len; i = ++_i) {
        line = _ref[i];
        if ((line != null ? line.trim() : void 0) !== '') {
          linePrefix = i === 0 ? prefix : '';
          $line = $(Mustache.render(this.consoleLineTemplate, {
            text: line,
            type: type,
            prefix: linePrefix
          }));
          $(".ironSP-console-prompt").before($line);
        }
      }
      return this.scrollToPrompt();
    };

    IronConsoleView.prototype.scrollToPrompt = function() {
      return this.$container.scrollTop(this.$container[0].scrollHeight);
    };

    IronConsoleView.prototype.toggleEditMode = function() {
      this.editMode = !this.editMode;
      if (this.editMode) {
        return this.$input.addClass('ironSP-console-edit');
      } else {
        return this.$input.removeClass('ironSP-console-edit');
      }
    };

    IronConsoleView.prototype.showExecuting = function(b) {
      if (b == null) {
        b = true;
      }
      if (b) {
        return this.$input.addClass('ironSP-console-executing');
      } else {
        return this.$input.removeClass('ironSP-console-executing');
      }
    };

    return IronConsoleView;

  })();

  window.IronConsole = IronConsole;

  window.IronConsoleView = IronConsoleView;

}).call(this);
