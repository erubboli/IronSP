class IronConsole
  constructor: (@serviceUrl, options = {}) ->
    @lastResultVariable = options['lastResultVariable'] || '_'
    @history = []
    @successCallbacks = []
    @errorCallbacks = []

  execute: (expression) ->
    @addToHistory expression
    wrapped = wrapExpression(expression)
    $.ajax
      type: 'POST',
      dataType: 'text',
      data: { expression: wrapped },
      url: @serviceUrl,
      success: (json) =>
        result = $.parseJSON(json)
        unless result["error"]?
          cb(result) for cb in @successCallbacks
        else
          cb(result["error"]) for cb in @errorCallbacks
      error: (args...) => cb(args...) for cb in @errorCallbacks

  getExpressionFromHistory: (index) ->
    @history[mod(index, @history.length)]

  onExecuteSuccess: (callback) ->
    @successCallbacks.push callback

  onExecuteError: (callback) ->
    @errorCallbacks.push callback

  addToHistory: (expression) =>
    escaped = expression.replace /\\n$/, ''
    @history.push escaped

  wrapExpression = (expression) ->
    if @lastResultVariable?
      "#{@lastResultVariable} = (#{expression});#{@lastResultVariable}"
    else
      expression

  mod = (n, base) -> ((n%base)+base)%base

class IronConsoleView
  constructor: (@console = new IronConsole, options = {}) ->
    @resultPrefix = options["resultPrefix"] || '=>'
    @inputPrefix = options["inputPrefix"] || '>>'
    @outputPrefix = options["outputPrefix"] || ''
    @$container = $(options["containerSelector"] || '#ironSP-console-container')
    @consoleTemplate = $(options["consoleTemplateSelector"] || '#ironSP-console-template').html()
    @consoleLineTemplate = $(options["consoleLineTemplateSelector"] || '#ironSP-console-line-template').html()
    @historyIndex = 0

    @render()
    @registerEventHandlers()

  render: =>
    @$console = Mustache.render(@consoleTemplate, prompt: @inputPrefix)
    @$container.empty().append @$console

  registerEventHandlers: =>
    @console.onExecuteSuccess (response) =>
      @append "output", @outputPrefix, response["output"] if response["output"]?
      @append "result", @resultPrefix, response["result"]
    @console.onExecuteError (error) =>
      @append "error", '', error

    @$input ||= $("#ironSP-console-input")
    @$input.keydown (e) =>
      expression = @$input.val()

      handled = true
      switch e.keyCode
        when 13 # Return
          if expression == 'clear'
            @clearConsole()
          else
            @append "input", @inputPrefix, expression
            @historyIndex = 0
            @clearInput()

            @console.execute expression
        when 9 # Tab
          insertTab()
        when 38 # Up
          @historyIndex += 1
          @$input.val @console.getExpressionFromHistory(@historyIndex)
        when 40 # Up
          @historyIndex -= 1
          @$input.val @console.getExpressionFromHistory(@historyIndex)
        else handled=false

      return !handled

  insertTab: =>
    @$input.val(@$input.val() + "    ")

  clearInput: ->
    @$input.val('')

  clearConsole: ->
    @clearInput()
    @$console.find(".ironSP-console-line").remove()

  caretPos: ->

  append: (type, prefix, text) =>
    for line in text.replace(/[\n|\r]+/gm, '\n').split('\n')
      if line?.trim() != ''
        $line = $(Mustache.render(@consoleLineTemplate, text: line, type: type, prefix: prefix))
        $(".ironSP-console-prompt").before($line)
