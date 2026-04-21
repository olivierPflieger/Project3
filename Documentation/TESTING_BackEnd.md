# DataShare Back-End - Tests unitaires et d'intégration

## Exécuter les tests

Pour lancer les tests sans couverture de code, ouvrez une console Powershell à la racine du projet DataShare_Tests 
et exécutez :

```
dotnet test
```

Optionnel: Pour ajouter le nom des tests, éxecutez :

```
dotnet test --logger "console;verbosity=normal"
```

Pour lancer les tests avec couverture de code, ouvrez une console Powershell à la racine du projet DataShare_Tests 
et exécutez :

```
dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings
```

puis, pour générer le rapport, exécutez : 

```
reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

Le rapport de couverture de code est disponible dans DataShare_Tests\coveragereport\index.html
