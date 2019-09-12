# NHibernate.ActsAsVersioned

This project aims to enable auditing of persistent classes in a form compatible with
the [acts_as_versioned](https://github.com/technoweenie/acts_as_versioned) rubygem for ActiveRecord.

The feature set is very limited. The main purpose of this package is to provide compatibility
with an existing Ruby on Rails code base that makes use of `acts_as_versioned`. If you are 
looking for a more sophisticated auditing solution for NHibernate, checkout 
[NHibernate Envers](https://github.com/nhibernate/nhibernate-envers).

## Usage

This description assumes that the reader is familiar with setting up NHibernate.

*Step 1* Reference the `NHibernate.ActsAsVersioned` assembly in your project.

*Step 2* Enable the acts as versioned functionality during program initialization, when the NHibernate `Configuration`
object is created.

```C#
	// Create configuration using XML mappings, mapping by code, Fluent NHibernate, etc
	Configuration cfg = BuildConfigurationSomehow();

	// Enable ActsAsVersioned
	cfg.IntegrateWithActsAsVersioned();

	// Now ready to build a session factory
	var sessionFactory = cfg.BuildSessionFactory();
```

The `IntegrateWithActsAsVersioned()` extension method is provided by the `NHibernate.ActsAsVersioned` assembly,
and is located in the `NHibernate.Cfg` namespace for convenience.

*Step 3* To mark an entity class as being versioned, use the `[ActsAsVersioned]` attribute. All persistent properties
of the entity will be versioned unless the property has been annotated with a `[NotVersioned]` attribute.

```C#
[ActsAsVersioned("book_versions")]
public class Book {
	public virtual int Id { get; set; }
	public virtual string Title { get; set; }
	public virtual Author Author { get; set; }

	[NotVersioned]
	public virtual string ScratchPad { get; set; }
}
```

The `[ActsAsVersioned]` attribute requires you to specify the table name for the version table. By convention
this table name is `{{entity}}_versions`. This goes against the Ruby on Rails convention of figuring names out
for itself, but at least for now the requirement to specify the table name eliminates the need for complicated
inflection algorithms.

## Limitations

This project was initiated for the purpose of providing compatibility with an existing Ruby
on Rails application. Only features that are required for that application have been implemented.

Features not supported:

- Abstract classes
- Subclasses
- Composite keys

Other limitations:

- The `acts_as_versioned` rubygem requires a `version` column. This implementation assumes that the
  class being tracked for changes has a `lock_version` column, and this is used in place of the version column.
  It would be simple enough to add a version column, but it has not been necessary to date.

## License

Because this project depends on [NHibernate](https://github.com/nhibernate/nhibernate-core), and was
written after inspecting the [NHibernate.Envers](https://github.com/nhibernate/nhibernate-envers) 
repository, this project is licensed using the [LGPL](LICENSE.txt).
