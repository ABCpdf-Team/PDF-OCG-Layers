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
using System.Net;
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;
using WebSupergoo.ABCpdf13.Operations;
using WebSupergoo.ABCpdf13.Atoms;


namespace OptionalContent {

	/// <summary>
	/// This class represents the Optional Content Properties Dictionary in a PDF document.
	/// This is the top level container of all Optional Content Groups (OCGs)
	/// in a document. OCGs are referenced at the top level in a global dictionary and their
	/// visibility states are held in a global configuration dictionary. These OCGs may then
	/// be referenced by one or more pages in the document. 
	/// Here we refer to OCGs simply as Groups.
	/// </summary>
	class Properties {
		/// <summary>
		/// Create a Properties object for the Doc.
		/// </summary>
		/// <param name="doc">The Doc on which to operate.</param>
		/// <param name="create">Whether to create a new properties entry if one is not already present.</param>
		/// <returns>The Properties object. This may be null if layers were not present in the document and the create parameter is false.</returns>
		public static Properties FromDoc(Doc doc, bool create) {
			Catalog catalog = doc.ObjectSoup.Catalog;
			Atom atom = catalog.Resolve(Atom.GetItem(catalog.Atom, "OCProperties"));
			if ((atom == null) && (create)) {
				int id = doc.AddObject("<< /OCGs [] /D << /BaseState /ON >> >>");
				IndirectObject io = doc.ObjectSoup[id];
				Atom.SetItem(catalog.Atom, "OCProperties", new RefAtom(io));
				atom = io.Atom;
			}
			if (atom == null)
				return null;
			return new Properties(catalog, atom);
		}

		private Catalog _catalog;
		private Atom _atom;

		private Properties(Catalog catalog, Atom atom) {
			_catalog = catalog;
			_atom = atom;
		}

		/// <summary>
		/// The Catalog from the Doc.
		/// </summary>
		public Catalog Catalog { get { return _catalog; } }

		/// <summary>
		/// Get the Groups for the Doc.
		/// </summary>
		/// <returns>A list of the Groups.</returns>
		public List<Group> GetGroups() {
			List<Group> list = new List<Group>();
			ArrayAtom array = EntryOCGs;
			if (array != null) {
				foreach (Atom item in array) {
					IndirectObject io = _catalog.ResolveObj(item);
					Group group = Group.FromIndirectObject(this, io);
					if (group != null)
						list.Add(group);
				}
			}
			return list;
		}

		/// <summary>
		/// Get the Optional Content Groups (OCGs) for the Page.
		/// </summary>
		/// <param name="page">The page from which OCGs should be found.</param>
		/// <returns>A list of the OCGs.</returns>
		public List<Group> GetGroups(Page page) {
			var ocs = new HashSet<IndirectObject>();
			var props = page.GetResourcesByType("Properties", true, true, true, true, null);
			foreach (var atom in props) {
				IndirectObject oc = page.ResolveObj(atom);
				if (oc != null)
					ocs.Add(oc);
			}
			foreach (var io in GetOptionalObjects(page))
				ocs.Add(GetOptionalObjectOC(io));
			var list = new List<Group>();
			foreach (var oc in ocs)
				AddGroups(oc, list);
			return list;
		}

		/// <summary>
		/// Adds any Optional Content Groups (OCGs) referenced by an OCG or OCG Membership Dictionary.
		/// </summary>
		/// <param name="io">The OCG or OCGMD</param>
		/// <param name="groups">The list to which the items should be added.</param>
		public void AddGroups(IndirectObject io, List<Group> groups) {
			if (io == null)
				return;
			Group ocg = Group.FromIndirectObject(this, io);
			if (ocg != null) {
				groups.Add(ocg);
				return;
			}
			MembershipGroup mg = MembershipGroup.FromIndirectObject(this, io);
			if (mg != null) {
				foreach (var ocg2 in mg.PolicyGroups)
					groups.Add(ocg2);
			}
		}

		/// <summary>
		/// Find all the document level groups that are not used on pages.
		/// </summary>
		/// <returns>A list of groups.</returns>
		public HashSet<Group> RemoveUnusedGroups() {
			HashSet<Group> groups = GetUnusedGroups();
			if (groups.Count > 0) {
				HashSet<int> ids = new HashSet<int>();
				foreach (Group g in groups)
					ids.Add(g.IndirectObject.ID);
				RemoveRefsByID(_atom, ids, new HashSet<int>(), 0);
				RemoveAppSettings();
			}
			return groups;
		}

		private HashSet<Group> GetUnusedGroups() {
			HashSet<Group> all = new HashSet<Group>(GetGroups());
			foreach (Page page in Catalog.Pages.GetPageArrayAll()) {
				foreach (Group g in GetGroups(page))
					all.Remove(g);
				// We add these OC entries in because our page scanning only references objects which are used.
				// So if there's an image which isn't used on the page but is referenced in the resources we need to
				// see if it has an OC entry. Yes it's redundant and could perhaps be removed from the resources but then
				// it might be there for a reason. Difficult to second guess what might be intended.
				List<Group> ocs = new List<Group>();
				foreach (var io in GetOptionalObjects(page))
					AddGroups(GetOptionalObjectOC(io), ocs);
				foreach (Group g in ocs)
					all.Remove(g);
			}
			return all;
		}

		internal List<IndirectObject> GetOptionalObjects(Page page) {
			List<IndirectObject> ocs = new List<IndirectObject>();
			foreach (Atom atom in page.GetResourcesByType("XObject", true, true, true, true, null)) {
				IndirectObject oc = GetOptionalObjectOC(page.ResolveObj(atom));
				if (oc == null) continue;
				IndirectObject io = page.ResolveObj(atom);
				if (io == null) continue;
				ocs.Add(io);
			}
			foreach (Annotation annot in page.GetAnnotations()) {
				IndirectObject oc = GetOptionalObjectOC(annot);
				if (oc == null) continue;
				ocs.Add(annot);
			}
			return ocs;
		}

		internal IndirectObject GetOptionalObjectOC(IndirectObject io) {
			return io != null ? io.ResolveObj(MakeIndirect(io, io.Atom, "OC")) : null;
		}

		private void RemoveRefsByID(Atom atom, HashSet<int> ids, HashSet<int> seen, int depth) {
			if (depth > 100)
				throw new Exception("Atom nesting depth unfeasibly large.");
			Atom resAtom = _catalog.Resolve(atom);
			ArrayAtom array = resAtom as ArrayAtom;
			if (array != null) {
				int n = array.Count;
				for (int i = n - 1; i >= 0; i--) {
					Atom item = array[i];
					IndirectObject io = _catalog.ResolveObj(item);
					if (io != null) {
						if (ids.Contains(io.ID)) {
							array.RemoveAt(i);
							continue;
						}
						if (seen.Contains(io.ID))
							continue;
						seen.Add(io.ID);
					}
					Atom resItem = _catalog.Resolve(item);
					if (resItem is ArrayAtom || resItem is DictAtom)
						RemoveRefsByID(resItem, ids, seen, depth + 1);
				}
				return;
			}
			DictAtom dict = resAtom as DictAtom;
			if (dict != null) {
				foreach (string key in dict.GetKeys()) {
					Atom item = dict[key];
					IndirectObject io = _catalog.ResolveObj(item);
					if (io != null) {
						if (ids.Contains(io.ID)) {
							dict.Remove(key);
							continue;
						}
						if (seen.Contains(io.ID))
							continue;
						seen.Add(io.ID);
					}
					Atom resItem = _catalog.Resolve(item);
					if (resItem is ArrayAtom || resItem is DictAtom)
						RemoveRefsByID(resItem, ids, seen, depth + 1);
				}
				return;
			}
		}

		private void RemoveAppSettings() {
			// Adobe Illustrator holds separate layer info in the PieceInfo structure
			// and indeed other applications may also hold layer information here. 
			// It isn't ideal removing the entire thing because it will remove all
			// settings but then there isn't a public spec for AI so it's difficult
			// to know what else one would do which would be better.
			Atom.RemoveItem(Catalog.Atom, "PieceInfo");
			foreach (Page page in Catalog.Pages.GetPageArrayAll()) {
				Atom.RemoveItem(page.Atom, "PieceInfo");
				foreach (Atom atom in page.GetResourcesByType("XObject", true, true, true, true, null))
					Atom.RemoveItem(Catalog.Resolve(atom), "PieceInfo");
			}
		}

		/// <summary>
		/// Sort Groups for presentation in a UI. Remove any Groups which should not appear in UI. 
		/// Construct a list of indents for those Groups that should be presented nested.
		/// </summary>
		/// <param name="groups">The OCGs to order.</param>
		/// <param name="indents">A list of indents to indicate nestedness.</param>
		public void SortGroupsForPresentation(List<Group> groups, List<int> indents) {
			Dictionary<int, Group> lookup = new Dictionary<int, Group>();
			foreach (Group group in groups)
				lookup[group.IndirectObject.ID] = group;
			groups.Clear();
			indents.Clear();
			Configuration config = GetDefault();
			ArrayAtom order = config != null ? config.EntryOrder : null;
			if (order == null)
				return;
			SortGroupsForPresentation(order, lookup, groups, indents, 0);
		}

		private void SortGroupsForPresentation(ArrayAtom order, Dictionary<int, Group> lookup, List<Group> groups, List<int> indents, int depth) {
			if (depth > 100)
				throw new Exception("OCG order nesting depth unfeasibly large.");
			foreach (Atom atom in order) {
				Atom resAtom = _catalog.Resolve(atom);
				if (resAtom is ArrayAtom) { // OCG group
					SortGroupsForPresentation((ArrayAtom)resAtom, lookup, groups, indents, depth + 1);
				}
				else if (resAtom is DictAtom) { // OCG
					RefAtom refAtom = _catalog.ResolveRef(atom);
					if (refAtom != null) {
						Group group = null;
						lookup.TryGetValue(refAtom.ID, out group);
						if (group != null) {
							groups.Add(group);
							indents.Add(depth);
						}
					}
				}
			}
		}

		/// <summary>
		/// Add a Group to the Doc.
		/// </summary>
		/// <param name="name">The name for the Group</param>
		/// <param name="parent">The parent for the Group to indicate nested visibility. This may be null if nested visibility is not required.</param>
		/// <returns>The newly added Group.</returns>
		public Group AddGroup(string name, Group parent) {
			Group ocg = Group.NewGroup(this);
			ocg.EntryName = new StringAtom(name);
			if (true) { // we need to list the ocg in the global database
				ArrayAtom array = EntryOCGs;
				if (array == null) {
					array = new ArrayAtom();
					EntryOCGs = array;
				}
				array.Add(new RefAtom(ocg.IndirectObject));
			}
			if (true) { // we also need to list it in the visible entries
				Configuration config = GetDefault();
				ArrayAtom array = config.EntryOrder;
				if (array == null) {
					array = new ArrayAtom();
					config.EntryOrder = array;
				}
				if (parent != null) {
					Tuple<ArrayAtom, int> entry = FindArrayEntry(parent.IndirectObject, array, 0);
					if (entry == null)
						throw new ArgumentException("Parent OCG not present in configuration dictionary.");
					array = entry.Item1;
					int index = entry.Item2;
					ArrayAtom next = index < array.Count - 1 ? _catalog.Resolve(array[index + 1]) as ArrayAtom : null;
					if (next == null) {
						next = new ArrayAtom();
						array.Insert(index + 1, next);
					}
					array = next;
				}
				array.Add(new RefAtom(ocg.IndirectObject));
			}
			return ocg;
		}

		/// <summary>
		/// Add a Membership Group to the Doc.
		/// </summary>
		/// <returns>The newly added Membership Group.</returns>
		public MembershipGroup AddMembershipGroup() {
			return MembershipGroup.New(this);
		}

		/// <summary>
		/// Get the default Configuration to indicate which layers are visible or invisible.
		/// </summary>
		/// <returns>The Configuration.</returns>
		public Configuration GetDefault() {
			Atom defaultConfig = EntryD;
			return defaultConfig != null ? Configuration.FromAtom(this, defaultConfig) : null;
		}

		/// <summary>
		/// Get the alternate Configurations that may be used to indicate which layers are visible or invisible.
		/// Alternate configurations are indended for use under different circumstances - they represent alternate
		/// views of the pages.
		/// </summary>
		/// <returns>A list of Configurations.</returns>
		public List<Configuration> GetConfigs() {
			List<Configuration> list = new List<Configuration>();
			ArrayAtom configs = EntryConfigs;
			if (configs != null) {
				foreach (Atom item in configs) {
					Configuration occd = Configuration.FromAtom(this, item);
					list.Add(occd);
				}
			}
			return list;
		}

		/// <summary>
		/// The Optional Content Properties Dictionary OCG entry.
		/// </summary>
		public ArrayAtom EntryOCGs {
			get { return _catalog.Resolve(Atom.GetItem(_atom, "OCGs")) as ArrayAtom; }
			set { Atom.SetItem(_atom, "OCGs", value); }
		}

		/// <summary>
		/// The Optional Content Properties Dictionary D entry.
		/// </summary>
		public DictAtom EntryD {
			get { return _catalog.Resolve(Atom.GetItem(_atom, "D")) as DictAtom; }
			set { Atom.SetItem(_atom, "D", value); }
		}

		/// <summary>
		/// The Optional Content Properties Dictionary Configs entry.
		/// </summary>
		public ArrayAtom EntryConfigs {
			get { return _catalog.Resolve(Atom.GetItem(_atom, "Configs")) as ArrayAtom; }
			set { Atom.SetItem(_atom, "Configs", value); }
		}

		private static Tuple<ArrayAtom, int> FindArrayEntry(IndirectObject io, ArrayAtom array, int depth) {
			if (depth > 100)
				return null;
			int n = array.Count;
			for (int i = 0; i < n; i++) {
				Atom atom = array[i];
				ArrayAtom subArray = io.Resolve(atom) as ArrayAtom;
				if (subArray != null) {
					Tuple<ArrayAtom, int> result = FindArrayEntry(io, subArray, depth + 1);
					if (result != null)
						return result;
				}
				else {
					RefAtom refAtom = io.ResolveRef(atom);
					if ((refAtom != null) && (refAtom.ID == io.ID))
						return new Tuple<ArrayAtom, int>(array, i);
				}
			}
			return null;
		}

		public static RefAtom MakeResourceIndirect(Page page, IndirectObject container, string resourceType, string resourceName, Atom resourceAtom, ref Atom cache) {
			RefAtom refAtom = container.ResolveRef(resourceAtom);
			if (refAtom == null) {
				if (cache == null)
					cache = GetResourceDict(page, container, resourceType, true);
				IndirectObject prop = new IndirectObject();
				prop.Atom = resourceAtom;
				container.Doc.ObjectSoup.Add(prop);
				refAtom = (RefAtom)Atom.SetItem(cache, resourceName, new RefAtom(prop));
			}
			return refAtom;
		}

		public static RefAtom MakeIndirect(IndirectObject something, Atom container, string name) {
			container = something.Resolve(container);
			Atom value = Atom.GetItem(container, name);
			if (value == null || value is RefAtom)
				return (RefAtom)value;
			IndirectObject prop = new IndirectObject();
			prop.Atom = value;
			something.Doc.ObjectSoup.Add(prop);
			RefAtom refAtom = new RefAtom(prop);
			Atom.SetItem(container, name, refAtom);
			return refAtom;
		}

		public static DictAtom GetResourceDict(Page page, IndirectObject container, string resourceType, bool create) {
			DictAtom dict = null;
			Atom resAtom = null;
			if (container is FormXObject) {
				resAtom = container.Resolve(Atom.GetItem(container.Atom, "Resources"));
				if (resAtom == null)
					resAtom = Atom.SetItem(container.Atom, "Resources", new DictAtom());
			}
			else {
				resAtom = container.Resolve(Atom.GetItem(page.Atom, "Resources"));
				Debug.Assert(resAtom  != null, "Page does not contain Resources.");
			}
			dict = container.Resolve(Atom.GetItem(resAtom, resourceType)) as DictAtom;
			if ((dict == null) && (create))
				dict = (DictAtom)Atom.SetItem(resAtom, resourceType, new DictAtom());
			return dict;
		}
	}

	/// <summary>
	/// This class represents an Optional Content Group (OCG) in a PDF document.
	/// An OCG is a layer-like object that may be visible or invisible. Items on the page
	/// may belong to one or more than one OCG. Only if all OCGs that they belong to are visible
	/// are the items visible. OCGs are held at the Doc level in a global dictionary
	/// and then referenced at a Page level.
	/// </summary>
	class Group : IEquatable<Group> {
		/// <summary>
		/// Create a new Group for the document.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <returns>The new Group.</returns>
		public static Group NewGroup(Properties oc) {
			IndirectObject io = IndirectObject.FromString("<< /Type /OCG /Name () >>");
			oc.Catalog.Doc.ObjectSoup.Add(io);
			return FromIndirectObject(oc, io);
		}

		/// <summary>
		/// Create a list of Groups from a list of atoms that reference Optional Content Groups (OCGs) already existing in the Doc.
		/// Only those atoms that reference OCGs will be included in the list. Other atoms will be ignored.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="atoms">The list of atoms.</param>
		/// <returns>A list of Groups.</returns>
		public static List<Group> FromAtoms(Properties oc, IEnumerable<Atom> atoms) {
			List<Group> list = new List<Group>();
			foreach (Atom atom in atoms) {
				IndirectObject io = oc.Catalog.ResolveObj(atom);
				Group ocg = Group.FromIndirectObject(oc, io);
				if (ocg != null)
					list.Add(ocg);
			}
			return list;
		}

		/// <summary>
		/// Create a Group from an IndirectObject that already exists in the Doc.
		/// If the IndirectObject is not an Optional Content Group then null will be returned.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="io">The IndirectObject.</param>
		/// <returns>The Group.</returns>
		public static Group FromIndirectObject(Properties oc, IndirectObject io) {
			if (io == null)
				return null;
			Group ocg = new Group(oc, io);
			return ocg.AllOk() ? ocg : null;
		}

		private Properties _oc;
		private IndirectObject _io;

		private Group(Properties oc, IndirectObject io) {
			_oc = oc;
			_io = io;
		}

		public override bool Equals(object obj) {
			return Equals(obj as Group);
		}

		public bool Equals(Group other) {
			return other != null && _io.ID == other._io.ID;
		}

		public override int GetHashCode() {
			return _io.ID.GetHashCode();
		}

		private bool AllOk() {
			// check type
			NameAtom type = _io.Resolve(Atom.GetItem(_io.Atom, "Type")) as NameAtom;
			if ((type == null) || (type.Text != "OCG"))
				return false; // malformed entry - corrupt PDF
			// The optional intent can be either a name or an array of names.
			// To keep things simple we always use the array form.
			Atom intent = _io.Resolve(Atom.GetItem(_io.Atom, "Intent"));
			if (intent is NameAtom) {
				ArrayAtom array = new ArrayAtom();
				array.Add(intent);
				Atom.SetItem(_io.Atom, "Intent", array);
				intent = array;
			}
			return true;
		}

		/// <summary>
		/// The IndirectObject representing the Group.
		/// </summary>
		public IndirectObject IndirectObject {
			get { return _io; }
		}

		/// <summary>
		/// Indicates whether the Group is visible or not.
		/// </summary>
		public bool Visible {
			get { return _oc.GetDefault().GetVisibility(true, this); }
			set { _oc.GetDefault().SetVisibility(value, this); }
		}

		/// <summary>
		/// Adds the Group to a page.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public string AddToPage(Page page) {
			return page.AddResource(_io, "Properties", "OC" + _io.ID.ToString());
		}

		/// <summary>
		/// The Optional Content Group Dictionary Name entry.
		/// </summary>
		public StringAtom EntryName {
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "Name")) as StringAtom; }
			set { Atom.SetItem(_io.Atom, "Name", value); }
		}

		/// <summary>
		/// The Optional Content Group Dictionary Intent entry.
		/// </summary>
		public ArrayAtom EntryIntent {
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "Intent")) as ArrayAtom; }
			set { Atom.SetItem(_io.Atom, "Intent", value); }
		}

		/// <summary>
		/// The Optional Content Group Dictionary Usage entry.
		/// </summary>
		public DictAtom EntryUsage {
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "Usage")) as DictAtom; }
			set { Atom.SetItem(_io.Atom, "Usage", value); }
		}
	}

	/// <summary>
	/// This class represents an Optional Content Membership Dictionary (OCMD) in a PDF document.
	/// This is a bit of a mouthful and doesn't actually express what it is, which is a
	/// kind of metagroup. It expresses visibility dependent on the visibility of a 
	/// set of groups rather than directly being switched on and off by itself. OCMDs are not
	/// referenced at the document level - only at a page level. However of course the OCMD references
	/// OCGs which are themselves referenced at a document level.
	/// Membership Groups may express visibility either using a Policy or using a Visibility Expression.
	/// Policies are simpler to implement but limited in scope. Visibility Expressions are more complex
	/// but also more powerful.
	/// OCGs always have to be IndirectObjects since they are referenced from more than one place.
	/// OCMGs do not have to be IndirectObject - they can be simple Atoms - since they are only referenced from
	/// the Resources of the Page. However to make things simple we convert them to IndirectObjects when we
	/// find them.
	/// </summary>
	class MembershipGroup : IEquatable<MembershipGroup> {
		/// <summary>
		/// An enumeration representing the possibilities for a Policy based Membership Group.
		/// </summary>
		public enum PolicyEnum { AllOn, AnyOn, AllOff, AnyOff };

		/// <summary>
		/// An enumeration representing the possibilities for a Logic based Membership Group.
		/// </summary>
		public enum LogicEnum { And, Or, Not };

		/// <summary>
		/// Create a new Membership Group.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc</param>
		/// <returns>The Membership Group</returns>
		public static MembershipGroup New(Properties oc) {
			IndirectObject io = IndirectObject.FromString("<< /Type /OCMD >>");
			oc.Catalog.Doc.ObjectSoup.Add(io);
			return FromIndirectObject(oc, io);
		}

		/// <summary>
		/// Create a list of Membership Groups from a list of atoms that reference Optional Content Membership Groups (OCMGs) already existing in the Doc.
		/// Only those atoms that reference OCMGs will be included in the list. Other atoms will be ignored.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="atoms">The list of atoms.</param>
		/// <returns>A list of Membership Groups.</returns>
		public static List<MembershipGroup> FromAtoms(Properties oc, IEnumerable<Atom> atoms) {
			// Here we assume that all atoms passed in are references to an OCMG IndirectObject.
			// In our code this is always the case. However if you modify the code to take Atoms
			// read from another PDF then you need to tak account of the fact that OCMGs (unlike
			// OCGs) need not be IndirectObjects - they can be plain DictAtoms. If this is the case
			// you will need to use Properties.MakeResourceIndirect to convert these Atoms into
			// IndirectObjects.
			List<MembershipGroup> list = new List<MembershipGroup>();
			foreach (Atom atom in atoms) {
				IndirectObject io = oc.Catalog.ResolveObj(atom);
				MembershipGroup ocmg = MembershipGroup.FromIndirectObject(oc, io);
				if (ocmg != null)
					list.Add(ocmg);
			}
			return list;
		}

		/// <summary>
		/// Create a new Membership Group from an IndirectObject that already exists in the Doc.
		/// If the IndirectObject is not a Membership Group then null will be returned.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc</param>
		/// <param name="io">The IndirectObject.</param>
		/// <returns>The Group.</returns>
		public static MembershipGroup FromIndirectObject(Properties oc, IndirectObject io) {
			if (io == null)
				return null;
			MembershipGroup ocm = new MembershipGroup(oc, io);
			return ocm.AllOk() ? ocm : null;
		}

		private Properties _oc;
		private IndirectObject _io;

		private MembershipGroup(Properties oc, IndirectObject io) {
			_oc = oc;
			_io = io;
		}

		public override bool Equals(object obj) {
			return Equals(obj as MembershipGroup);
		}

		public bool Equals(MembershipGroup other) {
			return other != null && _io.ID == other._io.ID;
		}

		public override int GetHashCode() {
			return _io.ID.GetHashCode();
		}

		private bool AllOk() {
			// check type
			NameAtom type = _io.Resolve(Atom.GetItem(_io.Atom, "Type")) as NameAtom;
			if ((type == null) || (type.Text != "OCMD"))
				return false; // malformed entry - corrupt PDF
			// OCGs are always IndirectObjects because the OCProperties entry
			// in the document Catalog requires it to be so.
			// The optional OCGs can be either a dict or an array of dicts.
			// To keep things simple we always use the array form.
			Atom ocgs = Atom.GetItem(_io.Atom, "OCGs");
			if (_io.Resolve(ocgs) is DictAtom) {
				ArrayAtom array = new ArrayAtom();
				array.Add(ocgs);
				Atom.SetItem(_io.Atom, "OCGs", array);
			}
			return true;
		}

		/// <summary>
		/// The IndirectObject representing the Membership Group.
		/// </summary>
		public IndirectObject IndirectObject {
			get { return _io; }
		}

		/// <summary>
		/// Indicates whether the Membership Group is visible or not.
		/// </summary>
		public bool Visible {
			get { return EntryVE != null ? EvaluateVisibility(EntryVE, 0) : EvaluateVisibility(Policy, PolicyGroups); } 
		}

		private bool EvaluateVisibility(Atom ve, int depth) {
			if (depth > 100)
				return true;
			ve = _oc.Catalog.Resolve(ve);
			if (ve is ArrayAtom) {
				ArrayAtom array = (ArrayAtom)ve;
				int n = array.Count;
				string op = n > 0 ? Atom.GetName(_oc.Catalog.Resolve(Atom.GetItem(array, 0))) : "";
				if (op == LogicEnum.And.ToString()) {
					for(int i = 1; i < n; i++) {
						IndirectObject io = _oc.Catalog.ResolveObj(array[i]);
						if (io != null) {
							if (!EvaluateVisibility(io))
								return false;
						}
						else {
							if (!EvaluateVisibility(array[i], depth + 1))
								return false;
						}
					}
					return true;
				}
				else if (op == LogicEnum.Or.ToString()) {
					for(int i = 1; i < n; i++) {
						IndirectObject io = _oc.Catalog.ResolveObj(array[i]);
						if (io != null) {
							if (EvaluateVisibility(io))
								return true;
						}
						else {
							if (EvaluateVisibility(array[i], depth + 1))
								return true;
						}
					}
					return false;
				}
				else if ((op == LogicEnum.Not.ToString()) && (n > 1)) {
					IndirectObject io = _oc.Catalog.ResolveObj(array[1]);
					if (io != null)
						return !EvaluateVisibility(io);
					else
						return !EvaluateVisibility(array[1], depth + 1);
				} 
			}
			return true;
		}

		private bool EvaluateVisibility(PolicyEnum? policy, IEnumerable<Group> groups) {
			bool visible = true;
			if (policy == null)
				policy = PolicyEnum.AnyOn;
			if (groups == null)
				groups = new Group[0];
			if (policy == PolicyEnum.AnyOn) {
				visible = false;
				foreach (Group group in groups) {
					if (group.Visible) {
						visible = true;
						break;
					}
				}
			}
			else if (policy == PolicyEnum.AnyOff) {
				visible = false;
				foreach (Group group in groups) {
					if (!group.Visible) {
						visible = true;
						break;
					}
				}
			}
			else if (policy == PolicyEnum.AllOn) {
				visible = true;
				foreach (Group group in groups) {
					if (!group.Visible) {
						visible = false;
						break;
					}
				}
			}
			else if (policy == PolicyEnum.AllOff) {
				visible = true;
				foreach (Group group in groups) {
					if (group.Visible) {
						visible = false;
						break;
					}
				}
			}
			return visible;
		}

		private bool EvaluateVisibility(IndirectObject io) {
			Group group = Group.FromIndirectObject(_oc, io);
			return group != null ? group.Visible : true;
		}

		/// <summary>
		/// The Policy for the Membership Group. Null indicates that there is no Policy.
		/// </summary>
		public PolicyEnum? Policy {
			get { NameAtom a = EntryP; return a != null ? (PolicyEnum?)Enum.Parse(typeof(PolicyEnum), a.Text) : null; }
			set { EntryP = value.HasValue ? new NameAtom(value.ToString()) : null; }
		}

		/// <summary>
		/// The Groups on which the Policy will operate.
		/// </summary>
		public IEnumerable<Group> PolicyGroups {
			get {
				ArrayAtom array = EntryOCGs;
				return array != null ? Group.FromAtoms(_oc, EntryOCGs) : new List<Group>();
			}
			set {
				ArrayAtom array = new ArrayAtom();
				foreach (Group item in value)
					array.Add(new RefAtom(item.IndirectObject));
				EntryOCGs = array;
			}
		}

		/// <summary>
		/// Create a Visibility Expression showing how the visibility of a set of Groups should be combined.
		/// </summary>
		/// <param name="op">The logic operator used to combine the visibility.</param>
		/// <param name="groups">The set of Groups on which to operate. Visible Groups are true and invisible ones are false.</param>
		/// <returns>An ArrayAtom containing the Visibility Expression.</returns>
		public ArrayAtom MakeVisibilityExpression(LogicEnum op, IEnumerable<Group> groups) {
			ArrayAtom array = new ArrayAtom();
			array.Add(new NameAtom(op.ToString()));
			foreach (var group in groups)
				array.Add(new RefAtom(group.IndirectObject));
			return array;
		}

		/// <summary>
		/// Create a Visibility Expression showing how the visibility of a set of Visibility Expressions should be combined.
		/// </summary>
		/// <param name="op">The logic operator used to combine the visibility.</param>
		/// <param name="atoms">The set of groups on which to operate.</param>
		/// <returns>An ArrayAtom containing the Visibility Expression.</returns>
		public ArrayAtom MakeVisibilityExpression(LogicEnum op, IEnumerable<ArrayAtom> atoms) {
			ArrayAtom array = new ArrayAtom();
			array.Add(new NameAtom(op.ToString()));
			foreach (var atom in atoms)
				array.Add(atom);
			return array;
		}

		/// <summary>
		/// All the Groups on which the Membership Group depends.
		/// </summary>
		public IEnumerable<Atom> GetGroupReferences() {
			List<Atom> list = new List<Atom>();
			GetGroupReferences(EntryVE, list, 0);
			GetGroupReferences(EntryOCGs, list, 0);
			return list;
		}

		private IEnumerable<Atom> GetGroupReferences(ArrayAtom array, List<Atom> list, int depth) {
			if ((array == null) || (depth > 100))
				return list;
			foreach (Atom atom in array) {
				Atom item = _oc.Catalog.Resolve(atom);
				if (item is ArrayAtom)
					GetGroupReferences((ArrayAtom)item, list, depth + 1);
				else {
					RefAtom refAtom =_oc.Catalog.ResolveRef(atom);
					if (refAtom != null)
						list.Add(refAtom);
				}
			}
			return list;
		}

		/// <summary>
		/// Add the Membership Group to a Page.
		/// </summary>
		/// <param name="page">The Page to which this should be added.</param>
		/// <returns>The resource name used for this Membership Group.</returns>
		public string AddToPage(Page page) {
			return page.AddResource(_io, "Properties", "OC" + _io.ID.ToString());
		}

		/// <summary>
		/// The Optional Content Membership Dictionary OCGs entry.
		/// </summary>
		public ArrayAtom EntryOCGs { 
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "OCGs")) as ArrayAtom; }
			set { Atom.SetItem(_io.Atom, "OCGs", value); }
		}

		/// <summary>
		/// The Optional Content Membership Dictionary P entry.
		/// </summary>
		public NameAtom EntryP {
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "P")) as NameAtom; }
			set { Atom.SetItem(_io.Atom, "P", value); }
		}

		/// <summary>
		/// The Optional Content Membership Dictionary VE entry.
		/// </summary>
		public ArrayAtom EntryVE {
			get { return _io.Resolve(Atom.GetItem(_io.Atom, "VE")) as ArrayAtom; }
			set { Atom.SetItem(_io.Atom, "VE", value); }
		}
	}

	/// <summary>
	/// This class represents an Optional Content Configuration Dictionary (OCCD) in a PDF document.
	/// An OCCD expresses the visibility of the Groups in the document and also how they should be
	/// presented in a User Interface (UI).
	/// </summary>
	class Configuration {
		/// <summary>
		/// Create a Configuration from an Optional Content Configuration Dictionary (OCCD) Atom already existing in the Doc.
		/// If the Atom is not an OCCD then null will be returned.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="atom">The Atom.</param>
		/// <returns>The Configuration.</returns>
		public static Configuration FromAtom(Properties oc, Atom atom) {
			if ((atom == null) || (oc == null))
				return null;
			return new Configuration(oc, atom);
		}

		private Properties _oc;
		private Atom _atom;

		private Configuration(Properties oc, Atom atom) {
			_oc = oc;
			_atom = atom;
		}

		/// <summary>
		/// The Atom representing the Membership Group.
		/// </summary>
		public Atom Atom {
			get { return _atom; }
		}

		private enum Visibility { On, Off, Unchanged };

		/// <summary>
		/// Evaluate the visibility of the Group.
		/// </summary>
		/// <param name="isDefault">Whether this is the default configuration.</param>
		/// <param name="layer">The Group for which visibility should be evaluated.</param>
		/// <returns>Whether visible.</returns>
		public bool GetVisibility(bool isDefault, Group layer) {
			Visibility visibility =  Visibility.On;
			NameAtom baseState = isDefault ? null : EntryBaseState;
			if (baseState != null) {
				switch (baseState.Text) {
					case "OFF": visibility = Visibility.Off; break;
					case "Unchanged": visibility = Visibility.Unchanged; break;
				}
			}
			if (visibility != Visibility.On) {
				ArrayAtom array = EntryON;
				if (array != null) {
					foreach (Atom item in array) {
						IndirectObject io = _oc.Catalog.ResolveObj(item);
						if ((io != null) && (io.ID == layer.IndirectObject.ID)) {
							visibility = Visibility.On;
							break;
						}
					}
				}
			}
			else if (visibility != Visibility.Off) {
				ArrayAtom array = EntryOFF;
				if (array != null) {
					foreach (Atom item in array) {
						IndirectObject io = _oc.Catalog.ResolveObj(item);
						if ((io != null) && (io.ID == layer.IndirectObject.ID)) {
							visibility = Visibility.Off;
							break;
						}
					}
				}
			}
			return visibility != Visibility.Off;
		}

		/// <summary>
		/// Set the visibility of a Group.
		/// </summary>
		/// <param name="visible">Whether the Group should be visible.</param>
		/// <param name="layer">The Group for which visibility should be evaluated.</param>
		public void SetVisibility(bool visible, Group layer) {
			RemoveEntries(EntryOFF, layer.IndirectObject.ID);
			RemoveEntries(EntryON, layer.IndirectObject.ID);
			if (visible) {
				if (EntryON == null)
					EntryON = new ArrayAtom();
				EntryON.Add(new RefAtom(layer.IndirectObject));
			}
			else {
				if (EntryOFF == null)
					EntryOFF = new ArrayAtom();
				EntryOFF.Add(new RefAtom(layer.IndirectObject));
			}
			//layer.IndirectObject.Atom = layer.IndirectObject.Atom.Clone();
		}

		private void RemoveEntries(ArrayAtom array, int id) {
			int n = array != null ? array.Count : 0;
			for (int i = 0; i < n; i++) {
				IndirectObject item = _oc.Catalog.ResolveObj(array[i]);
				if ((item != null) && (item.ID == id)) {
					array.RemoveAt(i);
					i--;
					n--;
				}
			}
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary Name entry.
		/// </summary>		
		public StringAtom EntryName {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "Name")) as StringAtom; }
			set { Atom.SetItem(_atom, "Name", value); }
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary Creator entry.
		/// </summary>		
		public StringAtom EntryCreator {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "Creator")) as StringAtom; }
			set { Atom.SetItem(_atom, "Creator", value); }
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary BaseState entry.
		/// </summary>		
		public NameAtom EntryBaseState {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "BaseState")) as NameAtom; }
			set { Atom.SetItem(_atom, "BaseState", value); }
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary ON entry.
		/// </summary>		
		public ArrayAtom EntryON {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "ON")) as ArrayAtom; }
			set { Atom.SetItem(_atom, "ON", value); }
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary OFF entry.
		/// </summary>		
		public ArrayAtom EntryOFF {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "OFF")) as ArrayAtom; }
			set { Atom.SetItem(_atom, "OFF", value); }
		}

		/// <summary>
		/// The Optional Content Configuration Dictionary Order entry.
		/// </summary>		
		public ArrayAtom EntryOrder {
			get { return _oc.Catalog.Resolve(Atom.GetItem(_atom, "Order")) as ArrayAtom; }
			set { Atom.SetItem(_atom, "Order", value); }
		}	
	}

	/// <summary>
	/// Class for evaluating which parts of a page are visible and which parts are members
	/// of which Optional Content Groups or Membership dictionaries. Includes facilities
	/// for redacting elements that are invisible.
	/// </summary>
	class Reader {
		/// <summary>
		/// Create a Reader for a particular Page.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="page">The page to operate on.</param>
		/// <returns></returns>
		public static Reader FromPage(Properties oc, Page page) {
			Reader content = new Reader(oc, page);
			return content.AllOk() ? content : null;
		}

		[DebuggerDisplay("\\{ Offset = {Offset} Length = {Length} Command = {Command} \\}")]
		private class Op : IComparable {
			public Op(int offset, int length, string command, string textWidth) {
				Offset = offset;
				Length = length;
				Command = command;
				TextWidth = textWidth;
			}
			public int CompareTo(Object obj) {
				return this.Offset - ((Op)obj).Offset;
			}
			public int Offset;
			public int Length;
			public string Command;
			public string TextWidth;
		}

		private Properties _oc;
		private Page _page;
		private Dictionary<int, List<Op>> _ops;
		private Dictionary<int, Dictionary<int, Atom[]>> _states; // first int is stream ID, second is position in stream
		private Dictionary<int, IDictionary<string, Atom>> _rezMap;
		private Dictionary<int, List<int>> _lookups; // fast lookup list

		private Reader() {
			Debug.Assert(false);
		}

		private Reader(Properties oc, Page page) {
			_oc = oc;
			_page = page;
			_ops = null;
			_states = null;
			_rezMap = null;
			_lookups = null;
		}

		private bool AllOk() {
			_page.DeInline(true);
			_page.Flatten(true, false);
			// Theoretically a FormXObject could have different visibilty
			// at different times it is drawn. So you should really call 
			// _page.StampFormXObjects(true) to separate them all out. However
			// this might have quite an impact on size and it seems unlikely
			// so we don't currently do this.
			// This could be more efficiently done using the ContentStreamScanner class,
			// but we would have to take care of OC properties in XObjects and Annotations ourselves.
			ScanPage(_page.GetText(Page.TextType.SvgPlus2, true));
			return true;
		}

		private void ScanPage(string svg) {
			_ops = new Dictionary<int, List<Op>>();
			_states = new Dictionary<int, Dictionary<int, Atom[]>>();
			_rezMap = new Dictionary<int, IDictionary<string, Atom>>();
			_lookups = new Dictionary<int, List<int>>();
			Stack<Atom> names = new Stack<Atom>();
			Stack<int> depths = new Stack<int>();
			depths.Push(0);

			using (StringReader reader = new StringReader(svg)) {
				for (string item; (item = reader.ReadLine()) != null; ) {
					if (item.StartsWith("<", StringComparison.InvariantCultureIgnoreCase)) {
						// get object ID
						string pdf_Op = GetAttribute(item, "pdf_Op");
						string op = WebUtility.HtmlDecode(pdf_Op);
						if (op == null)
							continue;
						int streamID = Int32.Parse(GetAttribute(item, "pdf_StreamID"));
						int streamOffset = Int32.Parse(GetAttribute(item, "pdf_StreamOffset"));
						int streamLength = Int32.Parse(GetAttribute(item, "pdf_StreamLength"));
						string textWidth = GetAttribute(item, "pdf_w1000");
						List<Op> ops = GetOpsFromStreamID(streamID);
						// FormXObjects may be drawn multiple times on page. Don't need to keep
						// track of all the times this happens. Once is enough.
						if ((ops.Count == 0) || (ops[ops.Count - 1].Offset < streamOffset))
							ops.Add(new Op(streamOffset, streamLength, op, textWidth));
						bool isBDC = (op.EndsWith("BDC"));
						bool isBMC = (op.EndsWith("BMC"));
						bool isEMC = (op.EndsWith("EMC"));
						if (isBDC || isBMC || isEMC) {
							if (isBDC) {
								ArrayAtom array = (ArrayAtom)Atom.FromString("[" + op + "]");
								NameAtom type = array.Count > 0 ? (NameAtom)array[0] : null; // this must be a NameAtom
								NameAtom name = array.Count > 1 ? array[1] as NameAtom : null; // this may be NameAtom or DictAtom but if OC, NameAtom only
								if ((type != null) && (name != null) && (type.Text == "OC")) { // optional content
									depths.Push(0);
									IDictionary<string, Atom> map = GetResourceMapFromStreamID(streamID);
									names.Push(map[name.Text]);
									Dictionary<int, Atom[]> state = GetStateFromStreamID(streamID);
									state.Add(streamOffset, names.Count > 0 ? names.ToArray() : new Atom[0]);
								}
								else { // some other kind of marked content
									depths.Push(depths.Pop() + 1);
								}
							}
							else if (isBMC) { // some other kind of marked content
								depths.Push(depths.Pop() + 1);
							}
							else if (isEMC) {
								if (depths.Peek() == 0) {
									names.Pop(); // exception here indicates too many EMC without corresponding BDC
									Dictionary<int, Atom[]> state = GetStateFromStreamID(streamID);
									state.Add(streamOffset, names.Count > 0 ? names.ToArray() : new Atom[0]);
									depths.Pop();
								}
								else {
									depths.Push(depths.Pop() - 1);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Get all the Layers referenced on the current page.
		/// </summary>
		/// <returns>A list of Layers.</returns>
		public List<Layer> GetLayers() {
			HashSet<Atom> set = new HashSet<Atom>();
			foreach (KeyValuePair<int, Dictionary<int, Atom[]>> state1 in _states) {
				foreach (KeyValuePair<int, Atom[]> state2 in state1.Value) {
					foreach (Atom atom in state2.Value) {
						set.Add(atom);
					}
				}
			}
			foreach (var io in _oc.GetOptionalObjects(_page))
				set.Add(Atom.GetItem(io.Atom, "OC"));
			List<Layer> list = new List<Layer>();
			foreach (Atom atom in set) {
				IndirectObject io = _oc.Catalog.ResolveObj(atom);
				Group group = Group.FromIndirectObject(_oc, io);
				if (group != null) {
					list.Add(new Layer(group, null));
					continue;
				}
				MembershipGroup membershipGroup = MembershipGroup.FromIndirectObject(_oc, io);
				if (membershipGroup != null) {
					list.Add(new Layer(membershipGroup, null));
					continue;
				}
			}
			return list;
		}

		/// <summary>
		/// Get the Groups active at a particular point in a particular content stream.
		/// You can use this to establish the visibility of a PDF operator at a particular
		/// point in a stream.
		/// </summary>
		/// <param name="streamID">The ID of the StreamObject.</param>
		/// <param name="position">The offset in the stream.</param>
		/// <returns>A list of Layers</returns>
		public List<Layer> GetLayersFromStreamAndPosition(int streamID, int position) {
			List<Layer> list = new List<Layer>();
			Atom[] atoms = GetStateFromStreamAndPosition(streamID, position);
			foreach (Group group in Group.FromAtoms(_oc, atoms))
				list.Add(new Layer(group, null));
			foreach (MembershipGroup group in MembershipGroup.FromAtoms(_oc, atoms))
				list.Add(new Layer(group, null));
			return list;
		}

		/// <summary>
		/// Get the Groups active for an Image XObject, Form XObject or Annotation.
		/// You can use this to establish the visibility of particular object.
		/// </summary>
		/// <param name="io">The IndirectObject to check against.</param>
		/// <returns>A list of Layers</returns>
		public List<Layer> GetLayersFromObject(IndirectObject io) {
			List<Layer> list = new List<Layer>();
			if (io is FormXObject || io is PixMap) {
				var oc = _oc.GetOptionalObjectOC(io);
				if (oc != null) {
					Group group = Group.FromIndirectObject(_oc, oc);
					if (group != null)
						list.Add(new Layer(group, null));
					MembershipGroup membership = MembershipGroup.FromIndirectObject(_oc, oc);
					if (group != null)
						list.Add(new Layer(membership, null));
				}
			}
			return list;
		}

		/// <summary>
		/// Delete all content associated with the specified Group. After this the Reader will
		/// no longer be valid and will need to be recreated if further operations are required.
		/// </summary>
		/// <param name="reader">The reader. This will be set to null on exit to ensure that an invalid reader is not used.</param>
		/// <param name="layer">The Layer whos content should be deleted.</param>
		static public void Redact(ref Reader reader, Layer layer) {
			HashSet<Atom> kept = reader.Redact(layer);
			reader.RemoveResources(kept);
			reader = null; // invalid
		}

		private HashSet<Atom> Redact(Layer layer) {
			HashSet<Atom> kept = new HashSet<Atom>();
			foreach (KeyValuePair<int, Dictionary<int, Atom[]>> pair in _states) {
				List<Op> ops = _ops[pair.Key];
				Dictionary<int, Atom[]> state = GetStateFromStreamID(pair.Key);
				List<int> lookup = GetLookupFromStreamID(pair.Key);
				int n = lookup.Count, pos = 0;
				bool keeping = true;
				for (int i = 0; i < n; i++) {
					bool match = false;
					Atom[] ocs = state[lookup[i]]; // OCMG or OCMD
					foreach (Atom atom in ocs) {
						IndirectObject io = _page.ResolveObj(atom);
						if (io.ID == layer.IndirectObject.ID) {
							match = true;
							break;
						}
					}
					if (!match)// keep track of the ones left
						foreach (Atom atom in ocs)
							kept.Add(_oc.Catalog.Resolve(atom));
					if (match && keeping) {
						pos = lookup[i];
						keeping = false;
					}
					else if (!match && !keeping) {
						Redact(ops, pos, lookup[i]);
						keeping = true;
					}
				}
				if ((!keeping) && (ops.Count > 0)) // flush
					Redact(ops, pos, ops[ops.Count - 1].Offset + 1);
				StringBuilder sb = new StringBuilder();
				foreach (Op op in ops)
					sb.AppendLine(op.Command);
				StreamObject so = (StreamObject)_page.Doc.ObjectSoup[pair.Key];
				so.ClearData();
				so.SetText(sb.ToString());
				so.CompressFlate();
			}
			foreach (var io in _oc.GetOptionalObjects(_page)) {
				IndirectObject oc = _oc.GetOptionalObjectOC(io);
				if (oc.ID == layer.IndirectObject.ID) {
					if (io is Annotation) {
						ArrayAtom annots = _page.Resolve(Atom.GetItem(_page.Atom, "Annotations")) as ArrayAtom;
						for (int i = annots.Count - 1; i >= 0; i--) {
							IndirectObject a = _page.ResolveObj(Atom.GetItem(annots, i));
							if (a != null && a.ID == io.ID)
								annots.RemoveAt(i);
						}
					}
					// If an XObject is invisible on the page, it is invisible
					// everywhere, so we can just clear it.
					// As an alternative we could identify the 'Do' elements
					// during ScanPage and then remove those from the stream.
					// However then we would need to ensure we also removed
					// the resource and that is a more complex operation.
					else if (io is StreamObject) {
						((StreamObject)io).ClearData();
						Atom.RemoveItem(io.Atom, "Resources");
						Atom.RemoveItem(io.Atom, "OC");
						Atom.RemoveItem(io.Atom, "Metadata");
					}
				}
			}
			_states = null; // invalid
			_lookups = null; // invalid
			return kept;
		}

		private void RemoveResources(HashSet<Atom> kept) {
			foreach (KeyValuePair<int, IDictionary<string, Atom>> pair1 in _rezMap) {
				foreach (KeyValuePair<string, Atom> pair2 in pair1.Value) {
					Atom atom = _oc.Catalog.Resolve(pair2.Value);
					if (!kept.Contains(atom)) {
						IndirectObject resObj = _oc.Catalog.Doc.ObjectSoup[pair1.Key];
						DictAtom dict = Properties.GetResourceDict(_page, resObj, "Properties", false);
						Atom.RemoveItem(dict, pair2.Key);
					}
				}
			}
		}

		private void Redact(List<Op> ops, int pos1, int pos2) {
			HashSet<string> cmdMakePath = new HashSet<string>(new string[] { "m", "l", "c", "v", "y", "h", "re" });
			HashSet<string> cmdPaintPath = new HashSet<string>(new string[] { "S", "s", "F", "f", "f*", "B", "B*", "b", "b*" });
			HashSet<string> cmdClipPath = new HashSet<string>(new string[] { "W" }); // ignore "n"
			HashSet<string> cmdShow = new HashSet<string>(new string[] { "Do", "sh" });
			int depth = 0;
			Stack<int> depths = new Stack<int>();
			List<int> paints = new List<int>();
			// there should always be an exact match for the binary search
			int start = ops.BinarySearch(new Op(pos1, 0, null, null));
			int n = ops.Count;
			for (int i = start; i < n; i++) {
				Op op = ops[i];
				Debug.Assert((i != start) || (op.Command.EndsWith("BDC")));
				if (op.Offset > pos2)
					break;
				Debug.Assert(!((i == n - 1) || (ops[i + 1].Offset > pos2)) || (op.Command == "EMC"));
				string cmd = op.Command.TrimEnd();
				int p = cmd.Length - 1;
				while (p >= 0) {
					if ((char.IsWhiteSpace(cmd[p])) || (cmd[p] == ')') || (cmd[p] == ']') || (cmd[p] == '>')) {
						cmd = cmd.Substring(p + 1);
						break;
					}
					p--;
				}
				// here we remove any displayed content
				if (cmdShow.Contains(cmd))
					op.Command = "";
				// here we remove any path painting but keep clipping
				else if (cmdMakePath.Contains(cmd)) {
					paints.Add(i);
				}
				else if (cmdPaintPath.Contains(cmd)) {
					op.Command = "n"; // needed because of bugs in Acrobat
					foreach (int j in paints)
						ops[j].Command = "";
					paints.Clear();
				}
				else if (cmdClipPath.Contains(cmd)) {
					paints.Clear();
				}
				// here we remove the BDC / EMC entries
				else if (cmd == "BMC") {
					depth++;
				}
				else if (cmd == "BDC") {
					if (op.Command.StartsWith("/OC")) {
						op.Command = "";
						depths.Push(depth);
					}
					depth++;
				}
				else if (cmd == "EMC") {
					depth--;
					if (depths.Peek() == depth) {
						op.Command = "";
						depths.Pop();
					}
				}
				// here we remove the text showing commands but in a way that preserves
				// the offset of the text position albeit with invisible text
				else if ((cmd == "TJ") || (cmd == "Tj"))
					op.Command = "[" + op.TextWidth + "] TJ";
				else if (cmd == "\'")
					op.Command = "T* [" + op.TextWidth + "] TJ";
				else if (cmd == "\"") {
					int p1 = 0, p2 = 0;
					while (p1 < op.Command.Length - 1) {
						if (!char.IsWhiteSpace(op.Command[p1]))
							break;
						p1++;
					}
					while (p1 < op.Command.Length - 1) {
						if (char.IsWhiteSpace(op.Command[p1]))
							break;
						p1++;
					}
					p2 = p1;
					while (p2 < op.Command.Length - 1) {
						if (!char.IsWhiteSpace(op.Command[p2]))
							break;
						p2++;
					}
					while (p2 < op.Command.Length - 1) {
						if (char.IsWhiteSpace(op.Command[p2]))
							break;
						p2++;
					}
					string tw = p1 > 0 ? op.Command.Substring(0, p1) : "0";
					string tc = p2 > p1 ? op.Command.Substring(p1, p2 - p1) : "0";
					op.Command = tw + " Tw " + tc + " Tc T* [" + op.TextWidth + "] TJ";
				}
			}
		}

		private Atom[] GetStateFromStreamAndPosition(int streamID, int position) {
			Dictionary<int, Atom[]> state = GetStateFromStreamID(streamID);
			List<int> lookup = GetLookupFromStreamID(streamID);
			int pos = lookup.BinarySearch(position);
			if (pos >= 0) { // exact match
				return state[lookup[pos]];
			}
			else { // exact key not found
				pos = ~pos; // Bitwise complement of the first index in the list larger than the target.
				// But we want the one before that - so move back one. For that reason we shouldn't need to pin the top
				if (pos == 0) // no state
					return new Atom[0];
				pos = pos >= 0 ? pos - 1 : pos;
				return state[lookup[pos]];
			}
		}

		private static string GetAttribute(string item, string name) {
			int p1 = item.IndexOf(name);
			if (p1 < 0) return null;
			p1 = item.IndexOf('=', p1) + 1;
			if (p1 < 0) return null;
			p1 = item.IndexOf('\"', p1) + 1;
			if (p1 < 0) return null;
			int p2 = item.IndexOf('\"', p1);
			if (p2 < 0) return null;
			string attr = item.Substring(p1, p2 - p1);
			return attr;
		}

		private List<Op> GetOpsFromStreamID(int streamID) {
			List<Op> op = null;
			_ops.TryGetValue(streamID, out op);
			if (op == null) {
				op = new List<Op>();
				_ops[streamID] = op;
			}
			return op;
		}

		private Dictionary<int, Atom[]> GetStateFromStreamID(int streamID) {
			Dictionary<int, Atom[]> state = null;
			_states.TryGetValue(streamID, out state);
			if (state == null) {
				state = new Dictionary<int, Atom[]>();
				_states[streamID] = state;
			}
			return state;
		}

		private IDictionary<string, Atom> GetResourceMapFromStreamID(int streamID) {
			IDictionary<string, Atom> map = null;
			_rezMap.TryGetValue(streamID, out map);
			if (map == null) {
				IndirectObject io = _page.Doc.ObjectSoup[streamID];
				int id = io is FormXObject ? streamID : _page.ID;
				map = _page.GetResourceMap(id, "Properties");
				Atom cache = null; // Convert OCMD Atoms to IndirectObjects where necessary
				List<string> keys = new List<string>(map.Keys);
				foreach (string key in keys) {
					Atom atom = map[key];
					if (_oc.Catalog.ResolveRef(atom) == null) {
						RefAtom refAtom = Properties.MakeResourceIndirect(_page, io, "Properties", key, atom, ref cache);
						map[key] = refAtom;
					}
				}
				_rezMap[streamID] = map;
			}
			return map;
		}

		private List<int> GetLookupFromStreamID(int streamID) {
			List<int> lookup = null;
			_lookups.TryGetValue(streamID, out lookup);
			if (lookup == null) {
				Dictionary<int, Atom[]> state = GetStateFromStreamID(streamID);
				lookup = new List<int>(state.Keys);
				lookup.Sort();
				_lookups[streamID] = lookup;
			}
			return lookup;
		}
	}

	/// <summary>
	/// Class to represent a Layer on the page. A Layer represents a reference to a visibility
	/// object from within a Page. The visibilty object may be either a Group or
	/// on a Membership Group.
	/// </summary>
	class Layer {
		private Layer() { ResourceName = ""; }

		/// <summary>
		/// Create a Layer object from a Group.
		/// </summary>
		/// <param name="group">The Group.</param>
		/// <param name="name">The resource name for the Group.</param>
		public Layer(Group group, string name) { Group = group; ResourceName = name; }

		/// <summary>
		/// Create a Layer object from a Membership Group.
		/// </summary>
		/// <param name="group">The Membership Group.</param>
		/// <param name="name">The resource name for the Membership Group.</param>
		public Layer(MembershipGroup group, string name) { MembershipGroup = group; ResourceName = name; }

		/// <summary>
		/// The Group. Either this or the MembershipGroup property will be null. Only one can be populated.
		/// </summary>
		public Group Group { get; set; }

		/// <summary>
		/// The Membership Group. Either this or the Group property will be null. Only one can be populated.
		/// </summary>
		public MembershipGroup MembershipGroup { get; set; }

		/// <summary>
		/// The resource name used for this object.
		/// </summary>
		public string ResourceName { get; set; }

		/// <summary>
		/// The IndirectObject for this Layer.
		/// </summary>
		public IndirectObject IndirectObject { get { return Group != null ? Group.IndirectObject : (MembershipGroup != null ? MembershipGroup.IndirectObject : null); } }

		/// <summary>
		/// Indicates whether the Group is visible or not.
		/// </summary>
		public bool Visible { get { return Group != null ? Group.Visible : (MembershipGroup != null ? MembershipGroup.Visible : true); } }
	}
	
	/// <summary>
	/// Class for creating layers on a Page.
	/// </summary>
	class Writer {
		private Properties _oc;
		private Page _page;
		private int _depth = 0;

		private Writer() {
			Debug.Assert(false);
		}

		/// <summary>
		/// Create a new Writer for the Page.
		/// </summary>
		/// <param name="oc">The Properties object for the Doc.</param>
		/// <param name="page">The Page on which to operate.</param>
		public Writer(Properties oc, Page page) {
			_oc = oc;
			_page = page;
		}

		~Writer() {
			Debug.Assert(_depth == 0, "Unbalanced StartLayer and EndLayer calls.");
		}

		/// <summary>
		/// The depth of nested calls to StartLayer and EndLayer.
		/// </summary>
		public int Depth { get { return _depth; } }

		/// <summary>
		/// Add a reference to a Group for the current Page.
		/// </summary>
		/// <param name="group">The Group for which a reference should be added.</param>
		/// <returns>The Layer.</returns>
		public Layer AddGroup(Group group) {
			string resource = group.AddToPage(_page);
			return new Layer(group, resource);
		}

		/// <summary>
		/// Add a reference to a Membership Group for the current Page.
		/// </summary>
		/// <param name="group">The Membership Group for which a reference should be added.</param>
		/// <returns>The Layer.</returns>
		public Layer AddGroup(MembershipGroup group) {
			string resource = group.AddToPage(_page);
			return new Layer(group, resource);
		}

		/// <summary>
		/// Start writing content for a particular layer. Calls to StartLayer may be nested
		/// but each call to StartLayer must be balanced by a corresponding call to EndLayer.
		/// </summary>
		/// <param name="layer">The Layer to start.</param>
		public void StartLayer(Layer layer) {
			_page.AddLayer(MakeLayerStart(_page, layer.ResourceName));
			_depth++;
		}

		/// <summary>
		/// End writing content for a particular layer. Calls to StartLayer may be nested
		/// but each call to StartLayer must be balanced by a corresponding call to EndLayer.
		/// </summary>
		public void EndLayer() {
			_page.AddLayer(MakeLayerEnd(_page));
			_depth--;
		}

		private static StreamObject MakeLayerStart(Page page, string name) {
			byte[] data = ASCIIEncoding.UTF7.GetBytes("/OC /" + name + " BDC\r\n");
			return new StreamObject(page.Doc.ObjectSoup, data);
		}

		private static StreamObject MakeLayerEnd(Page page) {
			byte[] data = ASCIIEncoding.UTF7.GetBytes("EMC\r\n");
			return new StreamObject(page.Doc.ObjectSoup, data);
		}
	}
}
