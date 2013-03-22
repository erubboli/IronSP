class IronConsole
  constructor: (@serviceUrl, options = {}) ->
    @lastResultVariable = options['lastResultVariable'] || '_'
    @history = []
    @successCallbacks = []
    @errorCallbacks = []


  execute: (expression) ->
    @addToHistory expression
    wrapped = @wrapExpression(expression)
    url_params = location.search.substring(1)
    url_params = "&" + url_params if url_params
    $.ajax
      type: 'POST'
      dataType: 'text'
      data: { script: wrapped }
      url: @serviceUrl + url_params
      success: (json) =>
        result = $.parseJSON(json)
        unless result["HasError"]
          cb(result) for cb in @successCallbacks
        else
          cb(result["Error"]) for cb in @errorCallbacks
      error: (args...) => cb(args...) for cb in @errorCallbacks

  getExpressionFromHistory: (index) ->
    return '' if @history.length == 0
    @history[@history.length - 1 - mod(index, @history.length)]

  onExecuteSuccess: (callback) ->
    @successCallbacks.push callback

  onExecuteError: (callback) ->
    @errorCallbacks.push callback

  addToHistory: (expression) =>
    escaped = expression.replace /\\n$/, ''
    @history.push escaped

  wrapExpression : (expression) =>
    if @lastResultVariable?
      expression
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
    @editMode = false

    @render()
    @registerEventHandlers()

  render: =>
    @$console = $ Mustache.render(@consoleTemplate, prompt: @inputPrefix)
    @$container.empty().append @$console

  registerEventHandlers: =>
    @console.onExecuteSuccess (response) =>
      @append "output", @outputPrefix, response["Output"] if response["Output"]
      @append "result", @resultPrefix, "[" + response["ExecutionTime"] + "ms] " + response["ReturnValue"]
      @showExecuting false
    @console.onExecuteError (error, stackTrace) =>
      @append "error", '', error
      @showExecuting false

    @$input ||= $("#ironSP-console-input")
    @$input.keydown (e) =>
      expression = @$input.val()?.trim()

      handled = true
      if !@editMode
        switch e.keyCode
          when 13 # Return
            if expression == 'clear'
              @clearConsole()
            else if expression != ''
              @append "input", @inputPrefix, expression
              @historyIndex = 0
              @clearInput()
              @showExecuting()

              @console.execute expression
          when 9 # Tab
            @insertTab()
          when 38 # Up
            @$input.val @console.getExpressionFromHistory(@historyIndex)
            @historyIndex += 1
          when 40 # Down
            @historyIndex -= 1
            @$input.val @console.getExpressionFromHistory(@historyIndex)
          when 45, 126 # insert, `
            @toggleEditMode()
          else handled=false
      else 
        switch e.keyCode
          when 9 # Tab
            @insertTab()
          when 45, 126 # Insert, `
            @toggleEditMode()
          else handled=false

      return !handled

  insertTab: =>
    @$input.replaceSelection("    ")

  clearInput: ->
    @$input.val('')

  clearConsole: ->
    @clearInput()
    @$console.find(".ironSP-console-line").remove()

  append: (type, prefix, text) =>
    text ||= ''
    for line,i in text.replace(/\r/gm, '\n').split('\n')
      if line?.trim() != ''
        linePrefix = if i == 0 then prefix else ''

        $line = $(Mustache.render(@consoleLineTemplate, text: line, type: type, prefix: linePrefix))
        $(".ironSP-console-prompt").before($line)
    @scrollToPrompt()

  scrollToPrompt: =>
    @$container.scrollTop(@$container[0].scrollHeight);

  toggleEditMode: =>
    @editMode = !@editMode
    if @editMode
      @$input.addClass 'ironSP-console-edit'
    else
      @$input.removeClass 'ironSP-console-edit'

  showExecuting: (b = true) =>
    if b
      @$input.addClass 'ironSP-console-executing'
    else
      @$input.removeClass 'ironSP-console-executing'


window.IronConsole = IronConsole
window.IronConsoleView = IronConsoleView