﻿// CodeInput
// by WebCoder49
// Based on a CSS-Tricks Post

var codeInput = {
    usedTemplates: {
    },
    defaultTemplate: undefined,
    CodeInput: class extends HTMLElement { // Create code input element
        constructor() {
            super(); // Element
        }

        /* Syntax-highlighting functions */
        update(text) {

            if (this.value != text) this.value = text; // Change value attribute if necessary.
            if (this.querySelector("textarea").value != text) this.querySelector("textarea").value = text;


            let result_element = this.querySelector("pre code");

            // Handle final newlines (see article)
            if (text[text.length - 1] == "\n") {
                text += " ";
            }
            // Update code
            result_element.innerHTML = this.escape_html(text);
            if (this.autodetect) { // Autodetection
                result_element.className = ""; // CODE
                result_element.parentElement.className = ""; // PRE
            }
            // Syntax Highlight
            if (this.template.includeCodeInputInHighlightFunc) this.template.highlight(result_element, this);
            else this.template.highlight(result_element);
        }

        sync_scroll() {
            /* Scroll result to scroll coords of event - sync with textarea */
            let input_element = this.querySelector("textarea");
            let result_element = this.template.preElementStyled ? this.querySelector("pre") : this.querySelector("pre code");
            // Get and set x and y
            result_element.scrollTop = input_element.scrollTop;
            result_element.scrollLeft = input_element.scrollLeft;
        }

        check_tab(event) {
            if (event.key != "Tab" || !this.template.isCode) {
                return;
            }
            let input_element = this.querySelector("textarea");
            let code = input_element.value;
            event.preventDefault(); // stop normal

            if (!event.shiftKey && input_element.selectionStart == input_element.selectionEnd) {
                // Shift always means dedent - this places a tab here.
                let before_selection = code.slice(0, input_element.selectionStart); // text before tab
                let after_selection = code.slice(input_element.selectionEnd, input_element.value.length); // text after tab

                let cursor_pos = input_element.selectionEnd + 1; // where cursor moves after tab - moving forward by 1 char to after tab
                input_element.value = before_selection + "\t" + after_selection; // add tab char

                // move cursor
                input_element.selectionStart = cursor_pos;
                input_element.selectionEnd = cursor_pos;

            } else {
                let lines = input_element.value.split("\n");
                let letter_i = 0;

                let selection_start = input_element.selectionStart; // where cursor moves after tab - moving forward by 1 indent
                let selection_end = input_element.selectionEnd; // where cursor moves after tab - moving forward by 1 indent

                let number_indents = 0;
                let first_line_indents = 0;

                for (let i = 0; i < lines.length; i++) {
                    letter_i += lines[i].length + 1; // newline counted

                    console.log(lines[i], ": start", input_element.selectionStart, letter_i, "&& end", input_element.selectionEnd, letter_i - lines[i].length)
                    if (input_element.selectionStart <= letter_i && input_element.selectionEnd >= letter_i - lines[i].length) {
                        // Starts before or at last char and ends after or at first char
                        if (event.shiftKey) {
                            if (lines[i][0] == "\t") {
                                // Remove first tab
                                lines[i] = lines[i].slice(1);
                                if (number_indents == 0) first_line_indents--;
                                number_indents--;
                            }
                        } else {
                            lines[i] = "\t" + lines[i];
                            if (number_indents == 0) first_line_indents++;
                            number_indents++;
                        }

                    }
                }
                input_element.value = lines.join("\n");

                // move cursor
                input_element.selectionStart = selection_start + first_line_indents;
                input_element.selectionEnd = selection_end + number_indents;
            }

            this.update(input_element.value);
        }

        check_enter(event) {
            if (event.key != "Enter" || !this.template.isCode) {
                return;
            }
            event.preventDefault(); // stop normal

            let input_element = this.querySelector("textarea");
            let lines = input_element.value.split("\n");
            let letter_i = 0;
            let current_line = lines.length - 1;
            let new_line = "";
            let number_indents = 0;

            // find the index of the line our cursor is currently on
            for (let i = 0; i < lines.length; i++) {
                letter_i += lines[i].length + 1;
                if (input_element.selectionEnd <= letter_i) {
                    current_line = i;
                    break;
                }
            }

            // count the number of indents the current line starts with (up to our cursor position in the line)
            let cursor_pos_in_line = lines[current_line].length - (letter_i - input_element.selectionEnd) + 1;
            for (let i = 0; i < cursor_pos_in_line; i++) {
                if (lines[current_line][i] == "\t") {
                    number_indents++;
                } else {
                    break;
                }
            }

            // determine the text before and after the cursor and chop the current line at the new line break
            let text_after_cursor = "";
            if (cursor_pos_in_line != lines[current_line].length) {
                text_after_cursor = lines[current_line].substring(cursor_pos_in_line);
                lines[current_line] = lines[current_line].substring(0, cursor_pos_in_line);
            }

            // insert our indents and any text from the previous line that might have been after the line break
            for (let i = 0; i < number_indents; i++) {
                new_line += "\t";
            }
            new_line += text_after_cursor;

            // save the current cursor position
            let selection_start = input_element.selectionStart;
            let selection_end = input_element.selectionEnd;

            // splice our new line into the list of existing lines and join them all back up
            lines.splice(current_line + 1, 0, new_line);
            input_element.value = lines.join("\n");

            // move cursor to new position
            input_element.selectionStart = selection_start + number_indents + 1;  // count the indent level and the newline character
            input_element.selectionEnd = selection_end + number_indents + 1;

            this.update(input_element.value);
        }

        escape_html(text) {
            return text.replace(new RegExp("&", "g"), "&amp;").replace(new RegExp("<", "g"), "&lt;"); /* Global RegExp */
        }

        /* Callbacks */
        connectedCallback() {
            // Added to document
            this.template = codeInput.usedTemplates[this.getAttribute("template") || codeInput.defaultTemplate];
            if (this.template.preElementStyled) this.classList.add("code-input_pre-element-styled");

            /* Defaults */
            let lang = this.getAttribute("lang");
            let placeholder = this.getAttribute("placeholder") || this.getAttribute("lang") || "";
            let value = this.value || this.innerHTML || "";

            this.innerHTML = ""; // Clear Content

            /* Create Textarea */
            let textarea = document.createElement("textarea");
            textarea.placeholder = placeholder;
            textarea.value = value;
            textarea.setAttribute("spellcheck", "false");

            if (this.getAttribute("name")) {
                textarea.setAttribute("name", this.getAttribute("name")); // for use in forms
                this.removeAttribute("name");
            }

            textarea.setAttribute("oninput", "this.parentElement.update(this.value); this.parentElement.sync_scroll();");
            textarea.setAttribute("onscroll", "this.parentElement.sync_scroll();");
            textarea.setAttribute("onkeydown", "this.parentElement.check_tab(event); this.parentElement.check_enter(event);");

            this.append(textarea);

            /* Create pre code */
            let code = document.createElement("code");
            let pre = document.createElement("pre");
            pre.setAttribute("aria-hidden", "true"); // Hide for screen readers
            pre.append(code);
            this.append(pre);

            if (this.template.isCode) {
                if (lang != undefined && lang != "") {
                    code.classList.add("language-" + lang);
                }
                else this.autodetect = true // No lang attribute
            }

            /* Add code from value attribute - useful for loading from backend */
            this.update(value, this);
        }
        static get observedAttributes() {
            return ["value", "placeholder", "lang", "template"]; // Attributes to monitor
        }
        attributeChangedCallback(name, oldValue, newValue) {
            if (this.isConnected) {
                // This will sometimes be called before the element has been created, so trying to update an attribute causes an error.
                // Thanks to Kevin Loughead for pointing this out.
                switch (name) {

                    case "value":

                        // Update code
                        this.update(newValue);

                        break;

                    case "placeholder":
                        this.querySelector("textarea").placeholder = newValue;
                        break;
                    case "template":
                        this.template = codeInput.usedTemplates[newValue || codeInput.defaultTemplate];
                        if (this.template.preElementStyled) this.classList.add("code-input_pre-element-styled");
                        else this.classList.remove("code-input_pre-element-styled");
                        // Syntax Highlight
                        this.update(this.value);

                    case "lang":
                        let code = this.querySelector("pre code");
                        let textarea = this.querySelector("textarea");

                        // Case insensitive
                        oldValue = oldValue.toLowerCase();
                        newValue = newValue.toLowerCase();

                        // Remove old language class and add new
                        console.log("REMOVE", "language-" + oldValue);
                        code.classList.remove("language-" + oldValue); // From CODE
                        code.parentElement.classList.remove("language-" + oldValue); // From PRE
                        code.classList.remove("language-none"); // Prism
                        code.parentElement.classList.remove("language-none"); // Prism

                        if (newValue != undefined && newValue != "") {
                            code.classList.add("language-" + newValue);
                            console.log("ADD", "language-" + newValue);
                        } else {
                            // Autodetect - works with HLJS
                            this.autodetect = true;
                        }

                        if (textarea.placeholder == oldValue) textarea.placeholder = newValue;

                        this.update(this.value);
                }
            }

        }

        /* Value attribute */
        get value() {
            return this.getAttribute("value");
        }
        set value(val) {
            return this.setAttribute("value", val);
        }
        /* Placeholder attribute */
        get placeholder() {
            return this.getAttribute("placeholder");
        }
        set placeholder(val) {
            return this.setAttribute("placeholder", val);
        }
    },
    registerTemplate: function (template_name, template) {
        // Set default class
        codeInput.usedTemplates[template_name] = template;
        codeInput.defaultTemplate = template_name;
    },
    templates: {
        custom(highlight = function () { }, preElementStyled = true, isCode = true, includeCodeInputInHighlightFunc = false) {
            return {
                highlight: highlight,
                includeCodeInputInHighlightFunc: includeCodeInputInHighlightFunc,
                preElementStyled: preElementStyled,
                isCode: isCode,
            };
        },
        prism(prism) { // Dependency: Prism.js (https://prismjs.com/)
            return {
                includeCodeInputInHighlightFunc: false,
                highlight: prism.highlightElement,
                preElementStyled: true,
                isCode: true
            };
        },
        hljs(hljs) { // Dependency: Highlight.js (https://highlightjs.org/)
            return {
                includeCodeInputInHighlightFunc: false,
                highlight: hljs.highlightElement,
                preElementStyled: false,
                isCode: true
            };
        },
        characterLimit() {
            return {
                highlight: function (result_element, code_input) {

                    let character_limit = Number(code_input.getAttribute("data-character-limit"));

                    let normal_characters = code_input.escape_html(code_input.value.slice(0, character_limit));
                    let overflow_characters = code_input.escape_html(code_input.value.slice(character_limit));

                    result_element.innerHTML = `${normal_characters}<mark class="overflow">${overflow_characters}</mark>`;
                    if (overflow_characters.length > 0) {
                        result_element.innerHTML += ` <mark class="overflow-msg">${code_input.getAttribute("data-overflow-msg") || "(Character limit reached)"}</mark>`;
                    }
                },
                includeCodeInputInHighlightFunc: true,
                preElementStyled: true,
                isCode: false
            }
        },
        rainbowText(rainbow_colors = ["red", "orangered", "orange", "goldenrod", "gold", "green", "darkgreen", "navy", "blue", "magenta"], delimiter = "") {
            return {
                highlight: function (result_element, code_input) {
                    let html_result = [];
                    let sections = code_input.value.split(code_input.template.delimiter);
                    for (let i = 0; i < sections.length; i++) {
                        html_result.push(`<span style="color: ${code_input.template.rainbow_colors[i % code_input.template.rainbow_colors.length]}">${code_input.escape_html(sections[i])}</span>`);
                    }
                    result_element.innerHTML = html_result.join(code_input.template.delimiter);
                },
                includeCodeInputInHighlightFunc: true,
                preElementStyled: true,
                isCode: false,
                rainbow_colors: rainbow_colors,
                delimiter: delimiter
            }
        },
        selenese() {
            
            return {
                highlight: function (result_element, code_input) {
                    let html_result = [];
                    let lines = code_input.value.split('\n');

                    for (let lineNo = 0; lineNo < lines.length; lineNo++) {
                        let curLine = code_input.escape_html(lines[lineNo]);
                        if (curLine.trimStart().startsWith('#')) {
                            // Comment block
                            html_result.push(`<span class='code-comment'>${curLine}</span>\n`)
                        }
                        else {
                            let parts = curLine.split(/(\|)/g);
                            for (let partNo = 0; partNo < parts.length; partNo++) {
                                var curPart = parts[partNo];
                                if (partNo == 0) {
                                    // First part - command or similar, probably
                                    html_result.push(`<span class='code-keyword'>${curPart}</span>`)
                                }
                                else {
                                    if (partNo == parts.length - 1) {
                                        // Last part.  May have comment?
                                        let subParts = curPart.split(/(#)/g);
                                        html_result.push(`<span class='code-normal'>${subParts[0]}</span>`);
                                        if (subParts.length > 1) {
                                            for (let subid = 1; subid < subParts.length; subid++) {
                                                html_result.push(`<span class='code-comment'>${subParts[subid]}</span>`);
                                            }
                                        }
                                    }
                                    else {
                                        html_result.push(`<span class='code-normal'>${curPart}</span>`);
                                    }
                                }
                            }
                            html_result.push('\n'); // eol
                        }
                    }
                    result_element.innerHTML = html_result.join('');
                },
                includeCodeInputInHighlightFunc: true,
                preElementStyled: true,
                isCode: false
            }
        },
        cscallback() {
            return {
                highlight: function (result_element, code_input) {
                    result_element.innerHTML = DotNet.invokeMethod('Xandevelop.Wigwam.Fiddle', 'JStoCSCall', code_input.value);
                },
                includeCodeInputInHighlightFunc: true,
                preElementStyled: true,
                isCode: false
            }
        }
    }
}

customElements.define("code-input", codeInput.CodeInput); // Set tag