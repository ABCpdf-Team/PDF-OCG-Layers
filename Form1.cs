// ===========================================================================
//	©2013-2024 WebSupergoo. All rights reserved.
//
//	This source code is for use exclusively with the ABCpdf product with
//	which it is distributed, under the terms of the license for that
//	product. Details can be found at
//
//		http://www.websupergoo.com/
//
//	This copyright notice must not be deleted and must be reproduced alongside
//	any sections of code extracted from this module.
// ===========================================================================

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;
using WebSupergoo.ABCpdf13.Operations;
using WebSupergoo.ABCpdf13.Atoms;
using OptionalContent;


namespace OCGLayers
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form {
		private Button annotateButton;
		private Button removeButton;
		private TextBox textBox2;
		private PictureBox previewPictureBox;
		private CheckedListBox layersCheckedListBox;
		private ComboBox pageCombo;
		private Button openButton;
		private Button saveButton;
		private Button createButton1;
		private Button createButton2;
		private Button createButton3;
		private TextBox textBox1;
		private TextBox textBox3;
		private Button openExternallyButton;
		private Button purgeButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.annotateButton = new System.Windows.Forms.Button();
			this.removeButton = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.previewPictureBox = new System.Windows.Forms.PictureBox();
			this.layersCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.pageCombo = new System.Windows.Forms.ComboBox();
			this.openButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.createButton1 = new System.Windows.Forms.Button();
			this.createButton2 = new System.Windows.Forms.Button();
			this.createButton3 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.openExternallyButton = new System.Windows.Forms.Button();
			this.purgeButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// annotateButton
			// 
			this.annotateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.annotateButton.Location = new System.Drawing.Point(666, 505);
			this.annotateButton.Name = "annotateButton";
			this.annotateButton.Size = new System.Drawing.Size(186, 26);
			this.annotateButton.TabIndex = 2;
			this.annotateButton.Text = "Annotate Layers";
			this.annotateButton.Click += new System.EventHandler(this.annotateButton_Click);
			// 
			// removeButton
			// 
			this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.removeButton.Location = new System.Drawing.Point(666, 538);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(186, 76);
			this.removeButton.TabIndex = 3;
			this.removeButton.Text = "Delete Content of Invisible Layers and Remove From Page";
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// textBox2
			// 
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox2.Location = new System.Drawing.Point(19, 770);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(623, 22);
			this.textBox2.TabIndex = 5;
			// 
			// previewPictureBox
			// 
			this.previewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewPictureBox.Location = new System.Drawing.Point(19, 21);
			this.previewPictureBox.Name = "previewPictureBox";
			this.previewPictureBox.Size = new System.Drawing.Size(623, 730);
			this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.previewPictureBox.TabIndex = 6;
			this.previewPictureBox.TabStop = false;
			// 
			// layersCheckedListBox
			// 
			this.layersCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.layersCheckedListBox.FormattingEnabled = true;
			this.layersCheckedListBox.Location = new System.Drawing.Point(666, 57);
			this.layersCheckedListBox.Name = "layersCheckedListBox";
			this.layersCheckedListBox.Size = new System.Drawing.Size(186, 106);
			this.layersCheckedListBox.TabIndex = 7;
			this.layersCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.layersCheckedListBox_ItemCheck);
			// 
			// pageCombo
			// 
			this.pageCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pageCombo.FormattingEnabled = true;
			this.pageCombo.Location = new System.Drawing.Point(666, 21);
			this.pageCombo.Name = "pageCombo";
			this.pageCombo.Size = new System.Drawing.Size(186, 24);
			this.pageCombo.TabIndex = 8;
			this.pageCombo.SelectedIndexChanged += new System.EventHandler(this.pageCombo_SelectedIndexChanged);
			// 
			// openButton
			// 
			this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.openButton.Location = new System.Drawing.Point(19, 804);
			this.openButton.Name = "openButton";
			this.openButton.Size = new System.Drawing.Size(124, 26);
			this.openButton.TabIndex = 9;
			this.openButton.Text = "Load PDF...";
			this.openButton.Click += new System.EventHandler(this.openButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.saveButton.Location = new System.Drawing.Point(270, 804);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(124, 26);
			this.saveButton.TabIndex = 10;
			this.saveButton.Text = "Save PDF...";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// createButton1
			// 
			this.createButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton1.Location = new System.Drawing.Point(666, 291);
			this.createButton1.Name = "createButton1";
			this.createButton1.Size = new System.Drawing.Size(186, 27);
			this.createButton1.TabIndex = 11;
			this.createButton1.Text = "Create Simple Layers";
			this.createButton1.Click += new System.EventHandler(this.createButton1_Click);
			// 
			// createButton2
			// 
			this.createButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton2.Location = new System.Drawing.Point(666, 323);
			this.createButton2.Name = "createButton2";
			this.createButton2.Size = new System.Drawing.Size(186, 27);
			this.createButton2.TabIndex = 12;
			this.createButton2.Text = "Create Nested Layers";
			this.createButton2.Click += new System.EventHandler(this.createButton2_Click);
			// 
			// createButton3
			// 
			this.createButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton3.Location = new System.Drawing.Point(666, 354);
			this.createButton3.Name = "createButton3";
			this.createButton3.Size = new System.Drawing.Size(186, 27);
			this.createButton3.TabIndex = 13;
			this.createButton3.Text = "Create Membership Layers";
			this.createButton3.Click += new System.EventHandler(this.createButton3_Click);
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Enabled = false;
			this.textBox1.Location = new System.Drawing.Point(666, 204);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(186, 82);
			this.textBox1.TabIndex = 14;
			this.textBox1.Text = "The buttons below allow you to create different types of layer structure. Details" +
    " of the specific structures created can be found in the source code.";
			// 
			// textBox3
			// 
			this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox3.Enabled = false;
			this.textBox3.Location = new System.Drawing.Point(666, 406);
			this.textBox3.Multiline = true;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(186, 92);
			this.textBox3.TabIndex = 15;
			this.textBox3.Text = "Use the buttons below to annotate items on the page with their layer details and " +
    "also to delete and redact items on specific layers.";
			// 
			// openExternallyButton
			// 
			this.openExternallyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.openExternallyButton.Location = new System.Drawing.Point(666, 678);
			this.openExternallyButton.Name = "openExternallyButton";
			this.openExternallyButton.Size = new System.Drawing.Size(186, 27);
			this.openExternallyButton.TabIndex = 16;
			this.openExternallyButton.Text = "Open in Acrobat";
			this.openExternallyButton.UseVisualStyleBackColor = true;
			this.openExternallyButton.Click += new System.EventHandler(this.openExternallyButton_Click);
			// 
			// purgeButton
			// 
			this.purgeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.purgeButton.Location = new System.Drawing.Point(666, 621);
			this.purgeButton.Name = "purgeButton";
			this.purgeButton.Size = new System.Drawing.Size(186, 51);
			this.purgeButton.TabIndex = 17;
			this.purgeButton.Text = "Remove Unused Layers From Document";
			this.purgeButton.Click += new System.EventHandler(this.purgeButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(873, 856);
			this.Controls.Add(this.purgeButton);
			this.Controls.Add(this.openExternallyButton);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.createButton3);
			this.Controls.Add(this.createButton2);
			this.Controls.Add(this.createButton1);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.openButton);
			this.Controls.Add(this.pageCombo);
			this.Controls.Add(this.layersCheckedListBox);
			this.Controls.Add(this.previewPictureBox);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.annotateButton);
			this.Name = "Form1";
			this.Text = "Optional Content Demo";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private Doc _doc;
		List<Group> _groups;

		private void Form1_Load(object sender, EventArgs e) {
			string thePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
			string[] files = Directory.GetFiles(thePath, "*.pdf");
			foreach (string file in files) {
				if (!file.Contains("_out")) {
					try {
						LoadPDF(file);
						break;
					}
					catch {
					}
				}
			}
		}

		private void annotateButton_Click(object sender, EventArgs e) {
			if (_doc == null)
				return;
			Properties props = Properties.FromDoc(_doc, false);
			if (props == null)
				return;
			foreach (Page page in _doc.ObjectSoup.Catalog.Pages.GetPageArrayAll()) {
				_doc.Page = page.ID;
				Reader oc = Reader.FromPage(props, page);
				if (true) {
					TextOperation op = new TextOperation(_doc);
					op.PageContents.AddPages(_doc.PageNumber);
					string text = op.GetText();
					if (text.Length > 0) {
						IList<TextFragment> fragments = op.Select(0, text.Length);
						_doc.FontSize = 12;
						_doc.Width = 0.1;
						foreach (TextFragment fragment in fragments) {
							_doc.Rect.String = fragment.Rect.String;
							List<OptionalContent.Layer> states = oc.GetLayersFromStreamAndPosition(fragment.StreamID, fragment.StreamOffset);
							if (states.Count > 0)
								_doc.AddText(MakeDescription(states));
							_doc.FrameRect();
						}
					}
				}
				if (true) {
					// The ImageOperation only picks up images and not Form XObjects.
					// Until we have an update which allows us to see Form XObject location
					// too, we cannot annotate Form XObjects which are marked as optional.
					ImageOperation op = new ImageOperation(_doc);
					op.PageContents.AddPages(_doc.PageNumber);
					foreach (ImageProperties image in op.GetImageProperties()) {
						List<OptionalContent.Layer> states = oc.GetLayersFromObject(image.PixMap);
						if (states.Count > 0) {
							string txt = MakeDescription(states);
							foreach (ImageRendition rendition in image.Renditions) {
								rendition.Focus();
								_doc.AddText(txt);
								_doc.FrameRect();
							}
						}
					}
				}
				if (true) {
					foreach (Annotation annot in page.GetAnnotations()) {
						List<OptionalContent.Layer> states = oc.GetLayersFromObject(annot);
						if (states.Count > 0) {
							annot.Focus();
							_doc.AddText(MakeDescription(states));
							_doc.FrameRect();
						}
					}
				}
			}
			UpdatePreview();
		}

		private static string MakeDescription(List<OptionalContent.Layer> states) {
			string[] names = new string[states.Count];
			for (int i = 0; i < names.Length; i++)
				names[i] = states[i].Group != null ? states[i].Group.EntryName.Text : "Membership Dictionary";
			string txt = string.Join(" ", names);
			return txt;
		}

		private void removeButton_Click(object sender, EventArgs e) {
			if (_doc == null)
				return;
			try {
				Cursor.Current = Cursors.WaitCursor;
				Properties props = Properties.FromDoc(_doc, false);
				if (props == null)
					return;
				Page page = (Page)_doc.ObjectSoup[_doc.Page];
				Reader reader = Reader.FromPage(props, page);
				List<OptionalContent.Layer> layers = reader.GetLayers();
				foreach (OptionalContent.Layer layer in layers) {
					if (layer.Visible == false) {
						if (reader == null)
							reader = Reader.FromPage(props, page);
						Reader.Redact(ref reader, layer);
					}
				}
				UpdateLayers();
				UpdatePreview();
			}
			finally {
				Cursor.Current = Cursors.Default;
			}
		}

		private void purgeButton_Click(object sender, EventArgs e) {
			if (_doc == null)
				return;
			try {
				Cursor.Current = Cursors.WaitCursor;
				Properties props = Properties.FromDoc(_doc, false);
				if (props == null)
					return;
				HashSet<Group> unused = props.RemoveUnusedGroups();
				if (unused.Count > 0) {
					UpdateLayers();
					UpdatePreview();
				}
			}
			finally {
				Cursor.Current = Cursors.Default;
			}
		}

		private List<TemporaryFile> _tempFiles;
		private void openExternallyButton_Click(object sender, EventArgs e) {
			if (_tempFiles == null)
				_tempFiles = new List<TemporaryFile>();
			TemporaryFile temp = new TemporaryFile(".pdf");
			// when we save we try to keep the object ids constant
			_doc.SaveOptions.Linearize = false;
			_doc.SaveOptions.Remap = false;
			_doc.Save(temp.Path);
			_tempFiles.Add(temp);
			System.Diagnostics.Process.Start(temp.Path);
		}

		private void createButton1_Click(object sender, EventArgs e) {
			Doc doc = new Doc();
			Properties props = Properties.FromDoc(doc, true);
			List<Group> groups = new List<Group>();
			for (int i = 1; i < 4; i++)
				groups.Add(props.AddGroup("Layer " + i.ToString(), null));
			doc.FontSize = 36;
			doc.Rect.Inset(20, 20);
			for (int i = 0; i < 1; i++) {
				doc.Page = doc.AddPage();
				Page page = (Page)doc.ObjectSoup[doc.Page];
				Writer writer = new Writer(props, page);
				foreach (Group group in groups) {
					OptionalContent.Layer layer = writer.AddGroup(group);
					writer.StartLayer(layer);
					doc.AddText(group.EntryName.Text + "\r\n");
					writer.EndLayer();
					doc.AddText("Always Visible\r\n");
					writer.StartLayer(layer);
					doc.AddText(group.EntryName.Text + "\r\n\r\n");
					writer.EndLayer();
				}
				doc.Flatten();
			}
			LoadPDF(doc);
		}

		private void createButton2_Click(object sender, EventArgs e) {
			Doc doc = new Doc();
			Properties props = Properties.FromDoc(doc, true);
			Group parent = null;
			List<Group> groups = new List<Group>();
			// The Optional Content Group parent determines the nesting in the UI. 
			// The presentation of the nesting is separate from the actual nesting 
			// of visibility. In general you will want to ensure the two correspond
			// as in the code here.
			for (int i = 1; i <= 10; i++) {
				Group group = props.AddGroup("Layer " + i.ToString(), parent);
				groups.Add(group);
				parent = i == 5 ? null : group;
			}
			doc.Page = doc.AddPage();
			doc.FontSize = 36;
			doc.Rect.Inset(20, 20);
			Page page = (Page)doc.ObjectSoup[doc.Page];
			Writer writer = new Writer(props, page);
			// This determines the nesting of actual visibility. Here we ensure that this
			// corresponds to the hierarchy specified in the UI so that it works in an
			// obvious way.
			for (int i = 0; i < groups.Count; i++) {
				Group group = groups[i];
				OptionalContent.Layer layer = writer.AddGroup(group);
				if (i == 5)
					while (writer.Depth > 0)
						writer.EndLayer();
				writer.StartLayer(layer);
				doc.AddText(group.EntryName.Text + "\r\n");
			}
			while (writer.Depth > 0)
				writer.EndLayer();
			doc.Flatten();
			LoadPDF(doc);
		}

		private void createButton3_Click(object sender, EventArgs e) {
			Doc doc = new Doc();
			Properties props = Properties.FromDoc(doc, true);
			List<Group> groups = new List<Group>();
			for (int i = 1; i < 4; i++)
				groups.Add(props.AddGroup("Layer " + i.ToString(), null));
			// membership policies are simple to use but limited in scope
			MembershipGroup alloff = props.AddMembershipGroup();
			alloff.Policy = MembershipGroup.PolicyEnum.AllOff;
			alloff.PolicyGroups = groups;
			// membership visibility expressions are more complex but more powerful
			MembershipGroup mgve = props.AddMembershipGroup();
			ArrayAtom ve = mgve.MakeVisibilityExpression(MembershipGroup.LogicEnum.Or, groups);
			mgve.EntryVE = mgve.MakeVisibilityExpression(MembershipGroup.LogicEnum.Not, new ArrayAtom[] { ve });
			doc.FontSize = 36;
			doc.Rect.Inset(20, 20);
			for (int i = 0; i < 3; i++) {
				doc.Page = doc.AddPage();
				Page page = (Page)doc.ObjectSoup[doc.Page];
				Writer writer = new Writer(props, page);
				OptionalContent.Layer layer1 = writer.AddGroup(alloff);
				doc.AddText("The next line uses a Policy so that it is only visible if all layers are turned off...\r\n");
				writer.StartLayer(layer1);
				doc.AddText("I am normally invisible\r\n\r\n");
				writer.EndLayer();
				OptionalContent.Layer layer2 = writer.AddGroup(mgve);
				doc.AddText("The next line uses a Visibility Expression so that it is only visible if all layers are turned off...\r\n");
				writer.StartLayer(layer2);
				doc.AddText("I am normally invisible\r\n");
				writer.EndLayer();
				doc.Flatten();
			}
			LoadPDF(doc);
		}

		private void pageCombo_SelectedIndexChanged(object sender, EventArgs e) {
			_doc.PageNumber = pageCombo.SelectedIndex + 1;
			UpdateLayers();
			UpdatePreview();
		}

		private void LoadPDF(string file) {
			Doc doc = new Doc();
			doc.Read(file);
			LoadPDF(doc);
			textBox2.Text = file;
		}

		private class ComboBoxItem {
			public string Name;
			public int Value;
			public ComboBoxItem(string name, int value) {
				Name = name; Value = value;
			}
			public override string ToString() {
				return Name;
			}
		}

		private void LoadPDF(Doc doc) {
			_doc = doc;
			_groups = null;
			int n = _doc.PageCount;
			pageCombo.Items.Clear();
			for (int i = 1; i <= n; i++)
				pageCombo.Items.Add(new ComboBoxItem("Page " + i.ToString() + " of " + n.ToString(), i));
			pageCombo.SelectedIndex = 0;
			UpdateLayers();
			UpdatePreview();
		}

		private void UpdateLayers() {
			_groups = null;
			layersCheckedListBox.Items.Clear();
			if (_doc == null)
				return;
			Properties props = Properties.FromDoc(_doc, false);
			if (props == null)
				return;
			Configuration config = props.GetDefault();
			if (config == null)
				return;
			Page page = (Page)_doc.ObjectSoup[_doc.Page];
			List<Group> groups = props.GetGroups(page);
			List<int> indents = new List<int>();
			props.SortGroupsForPresentation(groups, indents);
			Doc doc = _doc;
			try {
				_doc = null;
				int n = groups.Count;
				for (int i = 0; i < n; i++) {
					Group group = groups[i];
					string indent = new string(' ', indents[i] * 3);
					layersCheckedListBox.Items.Add(indent + group.EntryName.Text, group.Visible);
				}
			}
			finally {
				_doc = doc;
				_groups = groups;
			}
		}

		private void UpdatePreview() {
			if (_doc == null) {
				previewPictureBox.Image = null;
				return;
			}
			_doc.Rect.String = _doc.MediaBox.String;
			double scale = previewPictureBox.Width / _doc.Rect.Width;
			_doc.Rendering.DotsPerInch = Math.Max(scale, 1.0) * 72.0;
			previewPictureBox.Image = _doc.Rendering.GetBitmap();
			//if (_doc.Rendering.Log.Length > 0)
			//	MessageBox.Show("Render warning: " + _doc.Rendering.Log);
		}

		private void layersCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e) {
			if ((_doc == null) || (_groups == null) || (e.NewValue == CheckState.Indeterminate))
				return;
			_groups[e.Index].Visible = e.NewValue == CheckState.Checked;
			UpdatePreview();
		}

		private void openButton_Click(object sender, EventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "PDF Files (.pdf)|*.pdf|AI Files (*.ai)|*.ai";
			dialog.FilterIndex = 1;
			if (dialog.ShowDialog() == DialogResult.OK)
				LoadPDF(dialog.FileName);
		}

		private void saveButton_Click(object sender, EventArgs e) {
			if (_doc == null)
				_doc = new Doc();
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
			dialog.FilterIndex = 1;
			dialog.FileName = "Untitled.pdf";
			if (dialog.ShowDialog() == DialogResult.OK)
				_doc.Save(dialog.FileName);
		}
	}

	internal sealed class TemporaryFile : IDisposable {
		private string mPath;

		private TemporaryFile() { }

		public TemporaryFile(string ext) {
			mPath = GetTempFilePath(ext);
		}

		~TemporaryFile() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// sealed class cannot introduce virtual method
		// warned if sealed class introduces protected member
		private /*virtual*/ void Dispose(bool disposing) {
			try {
				DeleteFile();
			}
			catch {
			}
		}

		public string Path { get { return mPath; } }

		public void DeleteFile() {
			if (mPath != null) {
				if (File.Exists(mPath))
					File.Delete(mPath);
				mPath = null;
			}
		}

		private static string GetTempFilePath(string ext) {
			if (string.IsNullOrWhiteSpace(ext))
				ext = ".dat";
			else if (!ext.StartsWith("."))
				ext = "." + ext;
			return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ext);
		}
	}
}
