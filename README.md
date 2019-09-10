# NHibernate.ActsAsVersioned

This project aims to enable auditing of persistent classes in a form compatible with
the [acts_as_versioned](https://github.com/technoweenie/acts_as_versioned) rubygem for ActiveRecord.

The feature set is very limited. The main purpose of this package is to provide compatibility
with an existing Ruby on Rails code base that makes use of `acts_as_versioned`. If you are 
looking for a more sophisticated auditing solution for NHibernate, checkout 
[NHibernate Enverse](https://github.com/nhibernate/nhibernate-envers).

## License

Because this project depends on [NHibernate](https://github.com/nhibernate/nhibernate-core), and was
written after inspecting the [NHibernate.Enverse](https://github.com/nhibernate/nhibernate-envers) 
repository, this project is licensed using the [LGPL](LICENSE.txt).
