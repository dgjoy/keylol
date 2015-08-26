/**
 * @license Copyright (c) 2003-2015, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
	config.language = "zh-cn";
	config.toolbar = [["Bold", "Italic", "Underline", "Blockquote", "Link", "Image", "Embed"]];
	config.extraPlugins = "toolbarfixed";
};
