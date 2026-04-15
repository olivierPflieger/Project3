import { faker } from '@faker-js/faker'

describe('Files tests', () => {

    // fake datas    
    const email = faker.internet.email()
    const password = faker.internet.password()
    
    before(() => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        cy.log('fake user : ' + email + ', ' + password)
        
        // register new user
        cy.visit('/register')
        cy.get('[data-cy="register-input-email"]').type(email)
        cy.get('[data-cy="register-input-password"]').type(password)
        cy.get('[data-cy="register-input-confirmPassword"]').type(password)
        cy.get('[data-cy="register-button-submit"]').click()

        // assert
        cy.get('[data-cy="register-div-alert"]').should('contain.text', 'Inscription réussie !') 
        
        // Redirect to login page
        cy.get('[data-cy="register-link-reset"]').click()

        // login with the new user
        cy.get('[data-cy="login-input-email"]').type(email)
        cy.get('[data-cy="login-input-password"]').type(password)
        cy.get('[data-cy="login-button-submit"]').click()
        
        // assert
        cy.get('[data-cy="home-text"]').should('contain.text', 'Tu veux partager un fichier ?')
    })

    beforeEach(() => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // Got to home page
        cy.visit('/login')

        // login with the new user
        cy.get('[data-cy="login-input-email"]').type(email)
        cy.get('[data-cy="login-input-password"]').type(password)
        cy.get('[data-cy="login-button-submit"]').click()
        
        // assert
        cy.get('[data-cy="home-text"]').should('contain.text', 'Tu veux partager un fichier ?')
    })
    
    it('Upload file without any additional datas should work', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to upload page
        cy.get('[data-cy="home-button-televerser"]').click()
        
        // Création d'un fichier texte avec du contenu    
        cy.writeFile('cypress/downloads/test.txt', 'Contenu fake pour le test de téléversement de fichier')
        cy.get('[data-cy="file-form-input-file"]').selectFile(
            'cypress/downloads/test.txt',
            { force: true }
        )

        // Cliquer sur le bouton de téléversement
        cy.get('[data-cy="file-form-button-televerser"]').click()

        // Assert : vérifier que le message de succès s'affiche
        cy.get('[data-cy="file-form-div-success"]').should('contain.text', 'Félicitations, ton fichier sera conservé chez nous pendant 1 semaine !')

    })

    it('Upload file with additional datas should work', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to upload page
        cy.get('[data-cy="home-button-televerser"]').click()
        
        // Création d'un fichier texte avec du contenu    
        cy.writeFile('cypress/downloads/test.txt', 'Contenu fake pour le test de téléversement de fichier')
        cy.get('[data-cy="file-form-input-file"]').selectFile(
            'cypress/downloads/test.txt',
            { force: true }
        )

        // Ajout d'un mot de passe
        const password = faker.internet.password()
        cy.get('[data-cy="file-form-input-password"]').type(password)

        // Ajout d'une durée de conservation de 3 jours
        cy.get('[data-cy="file-form-select-expiration"]').select('3')

        // Ajout de tags
        const tag1 = faker.lorem.word()
        const tag2 = faker.lorem.word()

        cy.get('[data-cy="file-form-input-tags"]').type(tag1 + ' ' + tag2 + ' ')

        // Cliquer sur le bouton de téléversement
        cy.get('[data-cy="file-form-button-televerser"]').click()

        // Assert : vérifier que le message de succès s'affiche
        cy.get('[data-cy="file-form-div-success"]').should('contain.text', 'Félicitations, ton fichier sera conservé chez nous pendant 3 jour(s) !')
    })

    it('Upload blank file should return an error', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to upload page
        cy.get('[data-cy="home-button-televerser"]').click()
            
        // Cliquer sur le bouton de téléversement
        cy.get('[data-cy="file-form-button-televerser"]').click()

        // Assert
        cy.get('[data-cy="file-form-div-alert"]').should('contain.text', 'Aucun fichier valide n\'a été trouvé')

    })

    it('Upload file with bad password should return an error', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to upload page
        cy.get('[data-cy="home-button-televerser"]').click()
        
        // Création d'un fichier texte avec du contenu    
        cy.writeFile('cypress/downloads/test.txt', 'Contenu fake pour le test de téléversement de fichier')
        cy.get('[data-cy="file-form-input-file"]').selectFile(
            'cypress/downloads/test.txt',
            { force: true }
        )

        // Ajout d'un mot de passe    
        cy.get('[data-cy="file-form-input-password"]').type('123')
        
        // Cliquer sur le bouton de téléversement
        cy.get('[data-cy="file-form-button-televerser"]').click()

        // Assert
        cy.get('[data-cy="file-form-div-password-error"]').should('contain.text', 'Veuillez saisir un mot de passe d\'au moins 6 caractères')

    })

    it('Upload file then display files list and download the file should work', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to upload page
        cy.get('[data-cy="home-button-televerser"]').click()
        
        // Création d'un fichier texte avec du contenu    
        cy.writeFile('cypress/downloads/test-download.txt', 'Contenu fake pour le test de téléversement de fichier')
        cy.get('[data-cy="file-form-input-file"]').selectFile(
            'cypress/downloads/test-download.txt',
            { force: true }
        )

        // Ajout d'un mot de passe        
        cy.get('[data-cy="file-form-input-password"]').type('123456')

        // Ajout d'une durée de conservation de 3 jours
        cy.get('[data-cy="file-form-select-expiration"]').select('1')

        // Ajout de tags
        const tag1 = 'tag1'
        const tag2 = 'tag2'

        cy.get('[data-cy="file-form-input-tags"]').type(tag1 + ' ' + tag2 + ' ')

        // Cliquer sur le bouton de téléversement
        cy.get('[data-cy="file-form-button-televerser"]').click()

        // Assert : vérifier que le message de succès s'affiche
        cy.get('[data-cy="file-form-div-success"]').should('contain.text', 'Félicitations, ton fichier sera conservé chez nous pendant 1 jour(s) !')

        // go to monEspace page
        cy.get('[data-cy="app-button-monEspace"]').click()

        cy.contains('[data-cy="file-list-items"]', 'test-download.txt')
            .find('[data-cy="file-list-div-access"]')
            .click()

        // Assert
        cy.get('[data-cy="file-details-title"]').should('contain.text', 'Télécharger un fichier')

        // Download the file - without entering the password should return an error
        cy.get('[data-cy="file-details-button-telecharger"]').click()

        // Assert
        cy.get('[data-cy="file-details-div-alert"]').should('contain.text', 'Ce fichier est protégé par un mot de passe')

        // Enter the password
        cy.get('[data-cy="file-details-input-password"]').type('123456')

        // Download the file
        cy.get('[data-cy="file-details-button-telecharger"]').click()
        cy.get('[data-cy="file-details-div-alert"]').should('contain.text', 'Fichier correctement téléchargé!')
    })

    it('Display files list and delete a file should work', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        // go to monEspace page
        cy.get('[data-cy="app-button-monEspace"]').click()

        cy.get('[data-cy="file-list-items"]')
            .first()
            .find('[data-cy="file-list-div-delete"]')
            .click()

        // Assert
        cy.get('[data-cy="file-list-div-alert"]').should('contain.text', 'Fichier correctement supprimé')
    })

    it('Display files list return an error 500', () => {
        
        const url = Cypress.config('baseUrl');
        cy.log('url : ' + url)
        
        cy.intercept('GET', '/api/files', {
            statusCode: 500,
            body: {
                message: '500 - Internal Server Error'
            }
        }).as('getFilesError')

        // go to monEspace page
        cy.get('[data-cy="app-button-monEspace"]').click()

        cy.wait('@getFilesError')
        
        // Assert
        cy.get('[data-cy="file-list-div-alert"]').should('contain.text', '500 - Internal Server Error')
    })
})